using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProgressTrackerUserService.Data;
using ProgressTrackerUserService.Models;
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
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("view/{id}")]
        [Authorize]
        public async Task<IActionResult> ViewUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {id} not found.");
                return NotFound();
            }

            return Ok(new { user.UserId, user.FirstName, user.LastName, user.Email, user.University });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for Register.");
                return BadRequest(ModelState);
            }

            if (_context.Users.Any(u => u.Email == model.Email))
            {
                _logger.LogWarning($"Email already exists: {model.Email}");
                return Conflict("Email already exists.");
            }

            var user = new UserModel
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = SecurityLib.HashPassword(model.Password),
                University = model.University,
                DateCreated = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User registered: {user.Email}");
            return CreatedAtAction(nameof(ViewUser), new { id = user.UserId }, user);
        }

        [HttpGet("edit/{id}")]
        [Authorize]
        public async Task<IActionResult> GetEditUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {id} not found.");
                return NotFound();
            }

            var userEditModel = new UserEditModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                University = user.University
            };

            return Ok(userEditModel);
        }

        [HttpPut("edit")]
        [Authorize]
        public async Task<IActionResult> EditUser([FromBody] UserEditModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for EditUser.");
                return BadRequest(ModelState);
            }

            var existingUser = await _context.Users.FindAsync(model.UserId);
            if (existingUser == null)
            {
                _logger.LogWarning($"User with ID {model.UserId} not found.");
                return NotFound();
            }

            existingUser.FirstName = model.FirstName;
            existingUser.LastName = model.LastName;
            existingUser.Email = model.Email;
            existingUser.University = model.University;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                existingUser.PasswordHash = SecurityLib.HashPassword(model.Password);
            }

            _context.Update(existingUser);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User updated: {existingUser.Email}");
            return Ok(existingUser);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == model.Email);
            if (user == null || !SecurityLib.VerifyPassword(model.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid email or password.");
            }

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ProgressTrackerKey123456789123456789"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "yourdomain.com",
                audience: "yourdomain.com",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }
    }
}