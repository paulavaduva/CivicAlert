using CivicAlert.DTOs;
using CivicAlert.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CivicAlert.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IssuesController : ControllerBase
    {
        private readonly IIssueService _service;

        public IssuesController(IIssueService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllIssuesAsync());
        }

        [HttpPost]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] IssueCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _service.CreateIssueAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous] 
        public async Task<IActionResult> GetById(int id)
        {
            var issue = await _service.GetIssueByIdAsync(id);
            if (issue == null) return NotFound(new { message = "Issue not found." });
            return Ok(issue);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] IssueUpdateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            try
            {
                var result = await _service.UpdateIssueAsync(id, dto, userId!, isAdmin);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(); 
            }
        }

        [HttpPatch("{id}/validate")]
        [Authorize(Roles = "Dispatcher,Admin")]
        public async Task<IActionResult> Validate(int id, [FromBody] bool isApproved)
        {
            var dispatcherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _service.ValidateIssueAsync(id, dispatcherId!, isApproved);

            if (result == null) return NotFound();
            return Ok(new { message = "Issue status updated by dispatcher.", issue = result });
        }

        [HttpPatch("{id}/assign")]
        [Authorize(Roles = "HOD,Admin")]
        public async Task<IActionResult> Assign(int id, [FromBody] string teamLeaderId)
        {
            var result = await _service.AssignToTeamLeaderAsync(id, teamLeaderId);

            if (result == null) return NotFound();
            return Ok(new { message = "Issue successfully assigned to team leader.", issue = result });
        }

        [HttpPatch("{id}/complete")]
        [Authorize(Roles = "TeamLeader,Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Complete(int id, [FromForm] CompleteIssueDto dto)
        {
            if (dto.ResultImage == null) return BadRequest("The result image is required.");

            var result = await _service.CompleteIssueAsync(id, dto.ResultImage);

            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("staff-inbox")]
        [Authorize(Roles = "Admin,Dispatcher,HOD,TeamLeader")]
        public async Task<IActionResult> GetStaffInbox()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            var deptIdClaim = User.FindFirst("deptId")?.Value;
            int? deptId = !string.IsNullOrEmpty(deptIdClaim) ? int.Parse(deptIdClaim) : null;

            var issues = await _service.GetStaffInboxAsync(userId, role, deptId);
            return Ok(issues);
        }
    }
}
