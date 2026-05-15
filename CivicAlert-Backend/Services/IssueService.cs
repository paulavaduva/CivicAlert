using CivicAlert.DTOs;
using CivicAlert.Models;
using CivicAlert.Repositories;
using CivicAlert.Repositories.Interfaces;
using CivicAlert.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace CivicAlert.Services
{
    public class IssueService : IIssueService
    {
        private readonly IIssueRepository _repo;
        private readonly IFileService _fileService;
        private readonly ICategoryRepository _catRepo; 
        private readonly IDepartmentRepository _deptRepo; 
        private readonly GeminiService _geminiService;
        private readonly UserManager<User> _userManager;

        public IssueService(
            IIssueRepository repo, 
            IFileService fileService,
            ICategoryRepository catRepo,
            IDepartmentRepository deptRepo,
            GeminiService geminiService,
            UserManager<User> userManager)
        {
            _repo = repo;
            _fileService = fileService;
            _catRepo = catRepo;
            _deptRepo = deptRepo;
            _geminiService = geminiService;
            _userManager = userManager;
        }

        public async Task<IEnumerable<Issue>> GetAllIssuesAsync() => await _repo.GetAllAsync();

        public async Task<Issue> CreateIssueAsync(IssueCreateDto dto, string userId)
        {
            var nearbyIssues = await _repo.GetIssuesInRadiusAsync(dto.Latitude, dto.Longitude, 30);
            var nearbyUrls = nearbyIssues.Select(i => i.ImageUrl).ToList();

            using var ms = new MemoryStream();
            await dto.Image.CopyToAsync(ms);
            byte[] imageBytes = ms.ToArray();

            var categories = await _catRepo.GetAllAsync();
            var departments = await _deptRepo.GetAllAsync();

            var categoriesJson = JsonSerializer.Serialize(categories.Select(c => new { c.Id, c.Name }));
            var departmentsJson = JsonSerializer.Serialize(departments.Select(d => new { d.Id, d.Name }));

            var aiResult = await _geminiService.ProcessIssueAsync(
                imageBytes,
                dto.Description,
                categoriesJson,
                departmentsJson,
                nearbyUrls
            );

            if (aiResult.IsDuplicate)
            {
                throw new InvalidOperationException("Această problemă a fost deja raportată în această locație.");
            }

            int finalCategoryId;
            if (aiResult.IsNewCategory && !string.IsNullOrEmpty(aiResult.NewCategoryName))
            {
                var newCat = new Category
                {
                    Name = aiResult.NewCategoryName,
                    DepartmentId = aiResult.DepartmentId
                };
                await _catRepo.AddAsync(newCat);
                await _catRepo.SaveChangesAsync();
                finalCategoryId = newCat.Id;
            }
            else
            {
                finalCategoryId = aiResult.CategoryId ?? dto.CategoryId;
            }

            string imageUrl = await _fileService.UploadImageAsync(dto.Image);

            var issue = new Issue
            {
                Name = dto.Name,
                Description = aiResult.Summary + " | " + dto.Description,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Address = dto.Address,
                Severity = Enum.TryParse<IssueSeverity>(aiResult.Severity, true, out var sev) ? sev : (IssueSeverity)dto.Severity,
                CategoryId = finalCategoryId,
                ImageUrl = imageUrl,
                UserId = userId,
                Status = IssueStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,

                IsValid = aiResult.IsValid,
                AiConfidenceScore = aiResult.ConfidenceScore,
                AiValidationReason = aiResult.ValidationReason
            };

            if (!aiResult.IsValid)
            {
                issue.Status = IssueStatus.Rejected;
            }
            else if (aiResult.ConfidenceScore > 90)
            {
                issue.Status = IssueStatus.Validated;

                if (issue.Severity == IssueSeverity.Urgent)
                {
                    var bestTlId = await FindBestTeamLeaderAsync(issue.CategoryId, aiResult.DepartmentId);
                    if (bestTlId != null)
                    {
                        issue.AssignedToUserId = bestTlId;
                        issue.Status = IssueStatus.Assigned;
                    }
                }
            }
            else
            {
                issue.Status = IssueStatus.Pending;
            }

            await _repo.AddAsync(issue);
            await _repo.SaveChangesAsync();
            return issue;
        }

        public async Task<Issue?> PerformManualAutoAssignAsync(int issueId)
        {
            var issue = await _repo.GetByIdAsync(issueId);
            if (issue == null || issue.Category == null) return null;

            var bestTlId = await FindBestTeamLeaderAsync(issue.CategoryId, issue.Category.DepartmentId);
            if (bestTlId == null) return null;

            issue.AssignedToUserId = bestTlId;
            issue.Status = IssueStatus.Assigned;
            issue.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(issue);
            return issue;
        }

        public async Task<string?> FindBestTeamLeaderAsync(int categoryId, int deptId)
        {
            var teamLeaders = await _userManager.GetUsersInRoleAsync("TeamLeader");

            var deptTLs = teamLeaders.Where(u => u.DepartmentId == deptId).ToList();

            if (!deptTLs.Any()) return null;

            string? bestTlId = null;
            int maxScore = int.MinValue;

            foreach (var tl in deptTLs)
            {
                var tlIssues = await _repo.GetStaffIssuesAsync(userId: tl.Id);
                var activeTasks = tlIssues.Where(i => i.Status == IssueStatus.Assigned || i.Status == IssueStatus.InProgress).ToList();

                int score = 100;

                if (activeTasks.Any(t => t.CategoryId == categoryId)) score += 50;

                score -= (activeTasks.Count * 10);

                if (score > maxScore)
                {
                    maxScore = score;
                    bestTlId = tl.Id;
                }
            }
            return bestTlId;
        }

        public async Task<Issue?> GetIssueByIdAsync(int id) => await _repo.GetByIdAsync(id);
        public async Task<Issue?> UpdateIssueAsync(int id, IssueUpdateDto dto, string userId, bool isAdmin)
        {
            var existingIssue = await _repo.GetByIdAsync(id);
            if (existingIssue == null) return null;

            if (existingIssue.UserId != userId && !isAdmin)
            {
                throw new UnauthorizedAccessException("You do not have permission to modify this issue.");
            }

            existingIssue.Name = dto.Name ?? existingIssue.Name;
            existingIssue.Description = dto.Description ?? existingIssue.Description;
            existingIssue.Status = dto.Status;
            existingIssue.Severity = (IssueSeverity)dto.Severity;
            existingIssue.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(existingIssue);
            return existingIssue;
        }

        public async Task<Issue?> ValidateIssueAsync(int id, string dispatcherId, bool isApproved)
        {
            var issue = await _repo.GetByIdAsync(id);
            if (issue == null) return null;

            issue.Status = isApproved ? IssueStatus.Validated : IssueStatus.Rejected;
            issue.DispatcherId = dispatcherId;
            issue.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(issue);
            return issue;
        }

        public async Task<Issue?> AssignToTeamLeaderAsync(int id, string teamLeaderId)
        {
            var issue = await _repo.GetByIdAsync(id);
            if (issue == null) return null;

            issue.AssignedToUserId = teamLeaderId;
            issue.Status = IssueStatus.Assigned;
            issue.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(issue);
            return issue;
        }

        public async Task<Issue?> StartIssueAsync(int id)
        {
            var issue = await _repo.GetByIdAsync(id);
            if (issue == null) return null;

            if (issue.Status != IssueStatus.Assigned)
                return issue;

            issue.Status = IssueStatus.InProgress;
            issue.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(issue);
            return issue;
        }

        public async Task<Issue?> CompleteIssueAsync(int id, IFormFile resultImage)
        {
            var issue = await _repo.GetByIdAsync(id);
            if (issue == null) return null;

            var resolvedUrl = await _fileService.UploadImageAsync(resultImage);

            issue.ResolvedImageUrl = resolvedUrl;
            issue.Status = IssueStatus.Solved;
            issue.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(issue);
            return issue;
        }

        public async Task<IEnumerable<IssueDto>> GetStaffInboxAsync(string userId, string role, int? deptId)
        {
            IEnumerable<Issue> issues;

            if (role == "Dispatcher" || role == "Admin")
            {
                issues = await _repo.GetStaffIssuesAsync(status: IssueStatus.Pending);
            }
            else if (role == "HOD" && deptId.HasValue)
            {
                var deptIssues = await _repo.GetStaffIssuesAsync(deptId: deptId.Value);
                issues = deptIssues.Where(i =>
                    i.Status != IssueStatus.Pending &&
                    i.Status != IssueStatus.Rejected);

            }
            else if (role == "TeamLeader")
            {
                issues = await _repo.GetStaffIssuesAsync(userId: userId);
            }
            else
            {
                return Enumerable.Empty<IssueDto>();
            }

            return issues.Select(i => new IssueDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Address = i.Address,
                Latitude = i.Latitude,
                Longitude = i.Longitude,
                ImageUrl = i.ImageUrl,
                Status = i.Status.ToString(),
                Severity = i.Severity.ToString(),
                CreatedAt = (DateTime)i.CreatedAt,
                UpdatedAt = i.UpdatedAt,
                CategoryName = i.Category?.Name,
                Reporter = i.Reporter != null ? new UserDto
                {
                    Id = i.Reporter.Id,
                    FirstName = i.Reporter.FirstName,
                    LastName = i.Reporter.LastName
                } : null,
                AssignedToUser = i.AssignedToUser != null ? new UserDto()
                {
                    Id = i.AssignedToUser.Id,
                    FirstName = i.AssignedToUser.FirstName,
                    LastName = i.AssignedToUser.LastName
                } : null,
                ResolvedImageUrl = i.ResolvedImageUrl,
                IsValid = i.IsValid,
                AiConfidenceScore = i.AiConfidenceScore,
                AiValidationReason = i.AiValidationReason
            });
        }

        public async Task<IEnumerable<IssueDto>> GetUserIssuesAsync(string userId)
        {
            var issues = await _repo.GetIssuesByUserIdAsync(userId);

            return issues.Select(i => new IssueDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Status = i.Status.ToString(),
                CreatedAt = (DateTime)i.CreatedAt,
                UpdatedAt = i.UpdatedAt,
                CategoryName = i.Category?.Name,
                Address = i.Address,
                ImageUrl = i.ImageUrl
            }).OrderByDescending(i => i.CreatedAt);
        }
    }
}
