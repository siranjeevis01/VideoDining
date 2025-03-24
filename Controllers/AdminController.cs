using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoDiningApp.Services;
using VideoDiningApp.Models;
using System.ComponentModel.DataAnnotations;
using VideoDiningApp.Data;
using Microsoft.AspNetCore.SignalR;
using VideoDiningApp.Hubs;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

[Route("api/admin")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<AdminHub> _hubContext;

    public AdminController(IAdminService adminService, ApplicationDbContext context, IHubContext<AdminHub> hubContext)
    {
        _adminService = adminService;
        _context = context;
        _hubContext = hubContext;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardData()
    {
        var totalUsers = await _context.Users.CountAsync();
        var totalOrders = await _context.Orders.CountAsync();
        var totalFoodItems = await _context.FoodItems.CountAsync();
        var totalPayments = await _context.Payments.SumAsync(p => p.Amount); 

        return Ok(new
        {
            totalUsers,
            totalOrders,
            totalFoodItems,
            totalPayments
        });
    }

    [HttpPost("login")]
    public IActionResult AdminLogin([FromBody] AdminLoginRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new { message = "Email and password are required." });
        }

        var admin = _context.Admins.FirstOrDefault(a => a.Email == request.Email);
        if (admin == null)
        {
            return Unauthorized(new { message = "Admin not found." });
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash);
        if (!isPasswordValid)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        var token = GenerateJwtToken(admin);
        return Ok(new { token });
    }

    private string GenerateJwtToken(Admin admin)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("f6b4f7bb-748a-4572-92fc-bdc0e2f03a62a1f0ff9085ba8c545f5472e7e6b47653db8316f"); 

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.Name, admin.Email),
            new Claim(ClaimTypes.Role, "Admin")
        }),
            Expires = DateTime.UtcNow.AddHours(3),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _adminService.GetUsersAsync();
        return Ok(users ?? new List<User>()); 
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        bool deleted = await _adminService.DeleteUserAsync(id);
        if (!deleted) return NotFound();

        var users = await _adminService.GetUsersAsync();
        await _hubContext.Clients.All.SendAsync("userUpdated", users); 

        return NoContent();
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _adminService.GetOrdersAsync();
        return Ok(orders);
    }

    [HttpPut("orders/{orderId}")]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusRequest request)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
            return NotFound(new { message = "Order not found." });

        order.Status = request.Status;
        await _context.SaveChangesAsync();

        var orders = await _adminService.GetOrdersAsync();
        await _hubContext.Clients.All.SendAsync("orderUpdated", orders); 

        return Ok(new { message = "Order status updated." });
    }

    [HttpGet("foods")]
    public async Task<IActionResult> GetFoods()
    {
        var foods = await _adminService.GetFoodsAsync();
        return Ok(foods ?? new List<FoodItem>()); 
    }

    [HttpGet("foods/{id}")]
    public async Task<IActionResult> GetFood(int id)
    {
        var food = await _adminService.GetFoodByIdAsync(id);
        if (food == null)
            return NotFound();

        return Ok(food);
    }

    [HttpPost("foods")]
    public async Task<IActionResult> AddFood([FromBody] FoodDto foodDto)
    {
        var food = new FoodItem
        {
            Name = foodDto.Name,
            Price = foodDto.Price,
            Description = foodDto.Description,
            ImageUrl = foodDto.ImageUrl
        };

        var createdFood = await _adminService.AddFoodAsync(food);
        return CreatedAtAction(nameof(GetFood), new { id = createdFood.Id }, createdFood);
    }

    [HttpPut("foods/{id}")]
    public async Task<IActionResult> UpdateFood(int id, [FromBody] FoodDto foodDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var food = new FoodItem
        {
            Name = foodDto.Name,
            Price = foodDto.Price,
            Description = foodDto.Description,
            ImageUrl = foodDto.ImageUrl
        };

        var updated = await _adminService.UpdateFoodAsync(id, food);
        if (!updated) return NotFound();

        return NoContent();
    }

    [HttpDelete("foods/{id}")]
    public async Task<IActionResult> DeleteFood(int id)
    {
        var deleted = await _adminService.DeleteFoodAsync(id);
        if (!deleted) return NotFound();

        return NoContent();
    }

    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments()
    {
        var payments = await _adminService.GetAllPayments();
        return Ok(payments);
    }
}

public class AdminLoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class FoodDto
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }

    [Required]
    [Range(0.1, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
}

public class UpdateOrderStatusRequest
{
    [Required]
    public string Status { get; set; }
}
