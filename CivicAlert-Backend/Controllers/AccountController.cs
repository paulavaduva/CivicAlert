using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CivicAlert.Models;
using CivicAlert.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CivicAlert.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                Role = "User"
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                return Ok(new { message = "User registered successfully " });
            }

            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        [Authorize(Roles = "Admin,HOD")]
        [HttpPost("register-staff")]
        public async Task<IActionResult> RegisterStaff([FromBody] RegisterStaffDto model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            await EnsureRolesExist();

            var newUser = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber
            };

            if (User.IsInRole("Admin"))
            {
                newUser.Role = model.Role;
                newUser.DepartmentId = model.DepartmentId;
            }
            else if (User.IsInRole("HOD"))
            {
                newUser.Role = "TeamLeader";
                newUser.DepartmentId = currentUser.DepartmentId; 
            }

            var result = await _userManager.CreateAsync(newUser, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, newUser.Role);
                return Ok(new { message = $"{newUser.Role} account successfully created in department {newUser.DepartmentId}" });
            }

            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (result.Succeeded && user != null)
            {
                var token = await GenerateJwtToken(user);

                return Ok(new
                {
                    message = "Login successful!",
                    accessToken = token
                });
            }

            return Unauthorized(new { message = "Invalid email or password." });
        }

        [HttpGet("user-info")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserInfo()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            return Ok(new
            {
                email = user.Email,
                role = user.Role,
                departmentId = user.DepartmentId,
                firstName = user.FirstName,
                lastName = user.LastName
            });
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName),
                new Claim("role", user.Role),
                new Claim("deptId", user.DepartmentId?.ToString() ?? "0")
            };

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task EnsureRolesExist()
        {
            string[] roles = { "Admin", "Dispatcher", "HOD", "TeamLeader", "User" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
