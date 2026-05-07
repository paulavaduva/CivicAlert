using CivicAlert.Models;

namespace CivicAlert.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<Department>> GetAllDepartmentsAsync();
        Task<Department> CreateDepartmentAsync(string name);
    }
}
