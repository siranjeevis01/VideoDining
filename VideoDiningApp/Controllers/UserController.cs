using Microsoft.AspNetCore.Mvc;
using VideoDiningApp.Repositories;
using VideoDiningApp.Models;
using VideoDiningApp.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace VideoDiningApp.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IEmailService _emailService;
        private readonly IJwtService _jwtService;

        public UserController(IUserRepository userRepository, IPasswordService passwordService, IEmailService emailService, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _emailService = emailService;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO userDto)
        {
            if (userDto == null || string.IsNullOrEmpty(userDto.Email) || string.IsNullOrEmpty(userDto.Password))
                return BadRequest("Invalid user data.");

            if (!IsValidGmail(userDto.Email))
                return BadRequest("Invalid Gmail address. Please enter a valid Gmail account.");

            var existingUser = await _userRepository.GetUserByEmailAsync(userDto.Email);
            if (existingUser != null)
                return Conflict("User already exists.");

            var user = new User
            {
                Name = userDto.Name,
                Email = userDto.Email,
                Password = _passwordService.HashPassword(userDto.Password),
                PhoneNumber = userDto.PhoneNumber
            };

            await _userRepository.AddUserAsync(user);

            return Ok(new { message = "User registered successfully!" });
        }

        private bool IsValidGmail(string email)
        {
            return email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest loginDetails)
        {
            if (loginDetails == null)
                return BadRequest("Invalid login data.");

            var user = await _userRepository.GetUserByEmailAsync(loginDetails.Email);
            if (user == null)
                return NotFound("User not found.");

            if (!_passwordService.VerifyPassword(user.Password, loginDetails.Password))
                return Unauthorized("Invalid password.");

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                message = "Login successful!",
                token,
                userId = user.Id,
                username = user.Name
            });
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            return Ok(user);
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound("User not found.");

            return Ok(user);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromForm] UserUpdateDTO updatedUser, IFormFile? avatar)
        {
            if (updatedUser == null)
                return BadRequest("Invalid data.");

            var existingUser = await _userRepository.GetUserByIdAsync(updatedUser.Id);
            if (existingUser == null)
                return NotFound("User not found.");

            existingUser.Name = updatedUser.Name ?? existingUser.Name;
            existingUser.PhoneNumber = updatedUser.PhoneNumber ?? existingUser.PhoneNumber;

            if (!string.IsNullOrEmpty(updatedUser.NewPassword))
            {
                existingUser.Password = _passwordService.HashPassword(updatedUser.NewPassword);
            }

            if (avatar != null)
            {
                string uploadsFolder = Path.Combine("wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = $"{Guid.NewGuid()}_{avatar.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await avatar.CopyToAsync(stream);

                existingUser.Avatar = $"/uploads/{uniqueFileName}";
            }

            await _userRepository.UpdateUserAsync(existingUser);
            return Ok(new { message = "User profile updated successfully!", avatarUrl = existingUser.Avatar });
        }

        [HttpPost("reset-password-request")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest("Email is required.");

            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
                return NotFound("User not found.");

            var token = Guid.NewGuid().ToString();
            user.PasswordResetToken = token;
            user.TokenExpiry = DateTime.UtcNow.AddHours(1);

            await _userRepository.UpdateUserAsync(user);

            // Dynamically set frontend URL
            string frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:3000";
            string resetLink = $"{frontendUrl}/reset-password?token={token}";

            string emailBody = $"Click <a href='{resetLink}'>here</a> to reset your password.";
            await _emailService.SendEmailAsync(new List<string> { request.Email }, "Password Reset", emailBody);

            return Ok("Password reset email sent.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetDto resetDto)
        {
            var user = await _userRepository.GetUserByResetTokenAsync(resetDto.Token);
            if (user == null || user.PasswordResetToken == null || user.TokenExpiry < DateTime.UtcNow)
                return BadRequest("Invalid or expired token.");

            user.Password = _passwordService.HashPassword(resetDto.NewPassword);
            user.PasswordResetToken = null;
            user.TokenExpiry = null;

            await _userRepository.UpdateUserAsync(user);

            return Ok("Password updated successfully.");
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Invalid or missing token." });

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(user);
        }


        public class UserRegisterDTO
        {
            [Required] public string Name { get; set; } = string.Empty;
            [Required][EmailAddress] public string Email { get; set; } = string.Empty;
            [Required] public string Password { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
        }

        public class UserUpdateDTO
        {
            [Required] public int Id { get; set; }
            public string? Name { get; set; }
            public string? PhoneNumber { get; set; }
            public string? NewPassword { get; set; }
        }

        public class PasswordResetRequest
        {
            [Required, EmailAddress] public string Email { get; set; }
        }

        public class PasswordResetDto
        {
            [Required] public string Token { get; set; }
            [Required, MinLength(6)] public string NewPassword { get; set; }
        }
    }
}
