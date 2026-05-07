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
            => await _context.Issues.Include(i => i.Category).ToListAsync();

        public async Task<Issue?> GetByIdAsync(int id)
            => await _context.Issues.Include(i => i.Category).FirstOrDefaultAsync(i => i.Id == id);

        public async Task AddAsync(Issue issue) => await _context.Issues.AddAsync(issue);
        public async Task UpdateAsync(Issue issue)
        {
            _context.Issues.Update(issue);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
