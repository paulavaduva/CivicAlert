using CivicAlert.Models;

namespace CivicAlert.Repositories.Interfaces
{
    public interface IIssueRepository
    {
        Task<IEnumerable<Issue>> GetAllAsync();
        Task<Issue?> GetByIdAsync(int id);
        Task AddAsync(Issue issue);
        Task UpdateAsync(Issue issue);
        Task SaveChangesAsync();
        Task<IEnumerable<Issue>> GetStaffIssuesAsync(IssueStatus? status = null, int? deptId = null, string? userId = null);
        Task<IEnumerable<Issue>> GetIssuesByUserIdAsync(string userId);
        Task<IEnumerable<Issue>> GetIssuesInRadiusAsync(double lat, double lng, double radiusInMeters);
    }
}
