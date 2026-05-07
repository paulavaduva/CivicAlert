using CivicAlert.DTOs;
using CivicAlert.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CivicAlert.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;
        public CategoriesController(ICategoryService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _service.GetCategoriesAsync());

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
        {
            var result = await _service.CreateCategoryAsync(dto.Name, dto.DepartmentId);
            return Ok(result);
        }
    }
}
