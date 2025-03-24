using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VideoDiningApp.Data;
using VideoDiningApp.Models;
using VideoDiningApp.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace VideoDiningApp.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<CartHub> _cartHub;

        public CartService(ApplicationDbContext context, IHubContext<CartHub> cartHub)
        {
            _context = context;
            _cartHub = cartHub;
        }

        public async Task<List<CartItem>> GetCartByUserId(int userId)
        {
            return await _context.CartItems
                .Include(c => c.FoodItem)
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task AddToCart(int userId, int foodItemId, int quantity)
        {
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.FoodItemId == foodItemId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = userId,
                    FoodItemId = foodItemId,
                    Quantity = quantity,
                    Price = await _context.FoodItems.Where(f => f.Id == foodItemId).Select(f => f.Price).FirstOrDefaultAsync()
                };
                await _context.CartItems.AddAsync(cartItem);
            }

            await _context.SaveChangesAsync();
            await NotifyCartUpdate(userId);
        }

        public async Task RemoveItem(int userId, int foodItemId)
        {
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.FoodItemId == foodItemId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                await NotifyCartUpdate(userId);
            }
        }

        public async Task IncreaseQuantity(int userId, int foodItemId)
        {
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.FoodItemId == foodItemId);

            if (cartItem != null)
            {
                cartItem.Quantity++;
                await _context.SaveChangesAsync();
                await NotifyCartUpdate(userId);
            }
        }

        public async Task DecreaseQuantity(int userId, int foodItemId)
        {
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.FoodItemId == foodItemId);

            if (cartItem != null && cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
                await _context.SaveChangesAsync();
                await NotifyCartUpdate(userId);
            }
            else if (cartItem != null)
            {
                await RemoveItem(userId, foodItemId);
            }
        }

        private async Task NotifyCartUpdate(int userId)
        {
            var updatedCart = await GetCartByUserId(userId);
            await _cartHub.Clients.User(userId.ToString()).SendAsync("ReceiveCartUpdate", updatedCart);
        }
    }
}
