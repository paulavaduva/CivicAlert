using CivicAlert.Context;
using CivicAlert.Models;
using CivicAlert.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CivicAlert.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly CivicAlertContext _context;
        public CategoryRepository(CivicAlertContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Category>> GetAllAsync() => await _context.Categories.ToListAsync();
        public async Task<Category?> GetByIdAsync(int id) => await _context.Categories.FindAsync(id);
        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }
    }
}
