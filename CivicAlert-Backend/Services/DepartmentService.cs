using CivicAlert.Models;
using CivicAlert.Repositories.Interfaces;
using CivicAlert.Services.Interfaces;

namespace CivicAlert.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _repo;
        public DepartmentService(IDepartmentRepository repo) => _repo = repo;

        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync() => await _repo.GetAllAsync();

        public async Task<Department> CreateDepartmentAsync(string name)
        {
            var dept = new Department { Name = name };
            await _repo.AddAsync(dept);
            return dept;
        }
        public async Task<Department?> UpdateDepartmentAsync(int id, string name)
        {
            var dept = await _repo.GetByIdAsync(id);
            if (dept == null) return null;
            dept.Name = name;
            await _repo.UpdateAsync(dept);
            return dept;
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            var dept = await _repo.GetByIdAsync(id);
            if (dept == null) return false;
            await _repo.DeleteAsync(dept);
            return true;
        }
    }
}
