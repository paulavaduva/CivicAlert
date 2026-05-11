using CivicAlert.Context;
using CivicAlert.Models;
using CivicAlert.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CivicAlert.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly CivicAlertContext _context;
        public DepartmentRepository(CivicAlertContext context) => _context = context;

        public async Task<IEnumerable<Department>> GetAllAsync() =>
            await _context.Departments.Include(d => d.Categories).ToListAsync();

        public async Task<Department?> GetByIdAsync(int id) =>
            await _context.Departments.FindAsync(id);

        public async Task AddAsync(Department department)
        {
            await _context.Departments.AddAsync(department);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Department department)
        {
            _context.Departments.Update(department);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Department department)
        {
            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
        }
    }
}
