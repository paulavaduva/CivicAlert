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
                Severity = dto.Severity,
                CategoryId = dto.CategoryId,
                ImageUrl = imageUrl,
                UserId = userId,
                Status = IssueStatus.New, 
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
            existingIssue.Severity = dto.Severity;
            existingIssue.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(existingIssue);
            return existingIssue;
        }
    }
}
