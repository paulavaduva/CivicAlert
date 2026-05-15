using CivicAlert.Context;
using CivicAlert.Models;
using CivicAlert.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CivicAlert.Repositories
{
    public class IssueRepository : IIssueRepository
    {
        private readonly CivicAlertContext _context;
        public IssueRepository(CivicAlertContext context) => _context = context;

        public async Task<IEnumerable<Issue>> GetAllAsync()
            => await _context.Issues
                .Include(i => i.Category)
                .OrderByDescending(i => i.CreatedAt) 
                .ToListAsync();

        public async Task<Issue?> GetByIdAsync(int id)
            => await _context.Issues.Include(i => i.Category).FirstOrDefaultAsync(i => i.Id == id);

        public async Task AddAsync(Issue issue) => await _context.Issues.AddAsync(issue);
        public async Task UpdateAsync(Issue issue)
        {
            _context.Issues.Update(issue);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task<IEnumerable<Issue>> GetStaffIssuesAsync(IssueStatus? status = null, int? deptId = null, string? userId = null)
        {
            var query = _context.Issues
                .Include(i => i.Category)
                    .ThenInclude(c => c.Department)
                .Include(i => i.Reporter)
                .Include(i => i.AssignedToUser)
                .AsQueryable(); 

            if (status.HasValue)
            {
                query = query.Where(i => i.Status == status.Value);
            }

            if (deptId.HasValue)
            {
                query = query.Where(i => i.Category.DepartmentId == deptId.Value);
            }

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(i => i.AssignedToUserId == userId);
            }

            return await query
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Issue>> GetIssuesByUserIdAsync(string userId)
        {
            return await _context.Issues
                .Include(i => i.Category) 
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.CreatedAt) 
                .ToListAsync();
        }

        public async Task<IEnumerable<Issue>> GetIssuesInRadiusAsync(double lat, double lng, double radiusInMeters)
        {
            double delta = radiusInMeters / 111320.0;

            return await _context.Issues
                .Where(i => i.Status != IssueStatus.Solved && 
                            i.Latitude >= lat - delta && i.Latitude <= lat + delta &&
                            i.Longitude >= lng - delta && i.Longitude <= lng + delta)
                .ToListAsync();
        }
    }
}
