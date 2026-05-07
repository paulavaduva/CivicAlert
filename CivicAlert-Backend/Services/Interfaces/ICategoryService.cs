using CivicAlert.Models;

namespace CivicAlert.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<Category> CreateCategoryAsync(string name, int departmentId);
    }
}
