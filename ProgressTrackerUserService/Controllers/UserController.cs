using Microsoft.AspNetCore.Mvc;
using ProgressTrackerUserService.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using ProgressTrackerUserService.Data;
using ProgressTrackerUserService.Utils;
using ProgressTrackerUserService.ViewModels;

namespace ProgressTrackerUserService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(AppDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("view/{id}")]
        public async Task<IActionResult> ViewUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(new { user.UserId, user.FirstName, user.LastName, user.Email, user.University });
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.SingleOrDefault(u => u.Email == model.Email);
                if (user == null || !SecurityLib.VerifyPassword(model.Password, user.PasswordHash))
                {
                    _logger.LogWarning($"Invalid login attempt for email: {model.Email}");
                    return Unauthorized("Invalid email or password.");
                }

                _logger.LogInformation($"User logged in: {user.Email}");

                return Ok(new { user.UserId, user.FirstName, user.LastName, user.Email });
            }

            return BadRequest(ModelState);
        }
    }
}