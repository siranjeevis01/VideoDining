using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VideoDiningApp.Models;
using VideoDiningApp.Data;
using BCrypt.Net;
using Microsoft.AspNetCore.SignalR;
using VideoDiningApp.Hubs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace VideoDiningApp.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<AdminHub> _hubContext;

        public AdminService(ApplicationDbContext context, IHubContext<AdminHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<string> LoginAsync(LoginRequest request)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == request.Email);
            if (admin == null || !BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
            {
                return null; 
            }

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

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            return users ?? new List<User>(); 
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            var users = await GetUsersAsync();
            await _hubContext.Clients.All.SendAsync("userUpdated", users);

            return true;
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync()
        {
            return await _context.Orders.ToListAsync();
        }

        public async Task<List<FoodItem>> GetFoodsAsync()
        {
            return await _context.FoodItems.ToListAsync();
        }

        public async Task<FoodItem> GetFoodByIdAsync(int id)
        {
            return await _context.FoodItems.FindAsync(id);
        }

        public async Task<FoodItem> AddFoodAsync(FoodItem food)
        {
            _context.FoodItems.Add(food);
            await _context.SaveChangesAsync();
            return food;
        }

        public async Task<bool> UpdateFoodAsync(int id, FoodItem updatedFood)
        {
            var food = await _context.FoodItems.FindAsync(id);
            if (food == null) return false;

            food.Name = updatedFood.Name;
            food.Price = updatedFood.Price;
            food.Description = updatedFood.Description;
            food.ImageUrl = updatedFood.ImageUrl;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFoodAsync(int id)
        {
            var food = await _context.FoodItems.FindAsync(id);
            if (food == null) return false;

            _context.FoodItems.Remove(food);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.Status = status;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Payment>> GetAllPayments()
        {
            return await _context.Payments
                .Include(p => p.Order)
                .ThenInclude(o => o.User)
                .ToListAsync();
        }
    }
}
