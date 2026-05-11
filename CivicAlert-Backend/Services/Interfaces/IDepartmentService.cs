using CivicAlert.Models;

namespace CivicAlert.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<Department>> GetAllDepartmentsAsync();
        Task<Department> CreateDepartmentAsync(string name);
        Task<Department?> UpdateDepartmentAsync(int id, string name);
        Task<bool> DeleteDepartmentAsync(int id);
    }
}
