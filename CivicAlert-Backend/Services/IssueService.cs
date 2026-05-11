using CivicAlert.DTOs;
using CivicAlert.Models;
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

        public async Task<IEnumerable<Issue>> GetStaffInboxAsync(string userId, string role, int? deptId)
        {
            var allIssues = await _repo.GetStaffIssuesAsync();

            if (role == "Dispatcher" || role == "Admin")
            {
                return allIssues;
            }

            if (role == "HOD" && deptId.HasValue)
            {
                return allIssues.Where(i => i.Category?.DepartmentId == deptId.Value);
            }

            if (role == "TeamLeader")
            {
                return allIssues.Where(i => i.AssignedToUserId == userId);
            }

            return Enumerable.Empty<Issue>();
        }
    }
}
