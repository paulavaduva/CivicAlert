using CivicAlert.DTOs;
using CivicAlert.Models;
using System.Threading.Tasks;

namespace CivicAlert.Services.Interfaces
{
    public interface IIssueService
    {
        Task<IEnumerable<Issue>> GetAllIssuesAsync();
        Task<Issue> CreateIssueAsync(IssueCreateDto dto, string userId);
        Task<Issue?> GetIssueByIdAsync(int id);
        Task<Issue?> UpdateIssueAsync(int id, IssueUpdateDto dto, string userId, bool isAdmin);
        Task<Issue?> ValidateIssueAsync(int id, string dispatcherId, bool isApproved);
        Task<Issue?> AssignToTeamLeaderAsync(int id, string teamLeaderId);
        Task<Issue?> CompleteIssueAsync(int id, IFormFile resultImage);
        Task<IEnumerable<IssueDto>> GetStaffInboxAsync(string userId, string role, int? deptId);
        Task<Issue?> StartIssueAsync(int id);
        Task<IEnumerable<IssueDto>> GetUserIssuesAsync(string userId);
    }
}
