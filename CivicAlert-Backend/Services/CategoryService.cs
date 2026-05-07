using CivicAlert.Models;
using CivicAlert.Repositories.Interfaces;
using CivicAlert.Services.Interfaces;

namespace CivicAlert.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;
        public CategoryService(ICategoryRepository repo) => _repo = repo;

        public async Task<IEnumerable<Category>> GetCategoriesAsync() => await _repo.GetAllAsync();

        public async Task<Category> CreateCategoryAsync(string name, int departmentId)
        {
            var category = new Category
            {
                Name = name,
                DepartmentId = departmentId 
            };

            await _repo.AddAsync(category);
            return category;
        }
    }
}
