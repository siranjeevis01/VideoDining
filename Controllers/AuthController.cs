using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VideoDiningApp.Services;
using VideoDiningApp.Models;
using VideoDiningApp.Data;
using System;
using System.Threading.Tasks;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        try
        {
            var user = await _authService.RegisterAsync(registerRequest);
            var token = _authService.GenerateJwtToken(user); 
            return Ok(new { token });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        var user = await _authService.AuthenticateUser(model.Email, model.Password);
        if (user == null) return Unauthorized(new { message = "Invalid credentials" });

        var token = _authService.GenerateToken(user);

        return Ok(new
        {
            message = "Login successful",
            user,
            token
        });
    }

    [HttpGet("verify")]
    public IActionResult VerifyUser()
    {
        try
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { valid = false, message = "No token provided" });
            }

            var token = authHeader.Split(" ")[1]; 
            var principal = _authService.ValidateToken(token);

            if (principal == null)
            {
                return Unauthorized(new { valid = false, message = "Invalid or expired token" });
            }

            return Ok(new { valid = true, message = "Token is valid" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error verifying token: {ex.Message}");
            return StatusCode(500, new { valid = false, message = "Internal server error" });
        }
    }
}

public class RegisterRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
