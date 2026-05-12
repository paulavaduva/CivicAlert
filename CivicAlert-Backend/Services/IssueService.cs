using CivicAlert.DTOs;
using CivicAlert.Models;
using CivicAlert.Repositories;
using CivicAlert.Repositories.Interfaces;
using CivicAlert.Services.Interfaces;

namespace CivicAlert.Services
{
    public class IssueService : IIssueService
    {
        private readonly IIssueRepository _repo;
        private readonly IFileService _fileService;

        public IssueService(IIssueRepository repo, IFileService fileService)
        {
            _repo = repo;
            _fileService = fileService;
        }

        public async Task<IEnumerable<Issue>> GetAllIssuesAsync() => await _repo.GetAllAsync();

        public async Task<Issue> CreateIssueAsync(IssueCreateDto dto, string userId)
        {
            string? imageUrl = null;
            if (dto.Image != null)
            {
                imageUrl = await _fileService.UploadImageAsync(dto.Image);
            }

            var issue = new Issue
            {
                Name = dto.Name,
                Description = dto.Description,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Address = dto.Address,
                Severity = (IssueSeverity)dto.Severity,
                CategoryId = dto.CategoryId,
                ImageUrl = imageUrl,
                UserId = userId,
                Status = IssueStatus.Pending, 
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(issue);
            await _repo.SaveChangesAsync();
            return issue;
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
                ResolvedImageUrl = i.ResolvedImageUrl
            });
        }
    }
}
