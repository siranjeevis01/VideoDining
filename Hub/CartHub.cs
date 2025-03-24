using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using VideoDiningApp.Data;
using VideoDiningApp.Models;
using VideoDiningApp.Services;

public class CartHub : Hub
{
    private readonly ApplicationDbContext _context;
    private readonly ICartService _cartService;

    public CartHub(ApplicationDbContext context, ICartService cartService)
    {
        _context = context;
        _cartService = cartService;
    }

    public async Task SendCartUpdate(string userId)
    {
        await Clients.User(userId).SendAsync("ReceiveCartUpdate");
    }

    public async Task IncreaseQuantity(int userId, int itemId)
    {
        var cartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.FoodItemId == itemId);
        if (cartItem != null)
        {
            cartItem.Quantity++;
            await _context.SaveChangesAsync();
        }

        var updatedCart = await _cartService.GetCartByUserId(userId);
        await Clients.All.SendAsync("ReceiveCartUpdate", updatedCart);
    }

    public async Task DecreaseQuantity(int userId, int itemId)
    {
        var cartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.FoodItemId == itemId);
        if (cartItem != null && cartItem.Quantity > 1)
        {
            cartItem.Quantity--;
            await _context.SaveChangesAsync();
        }

        var updatedCart = await GetUserCart(userId);
        await Clients.User(userId.ToString()).SendAsync("ReceiveCartUpdate", updatedCart);
    }

    public async Task RemoveItem(int userId, int itemId)
    {
        var cartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.FoodItemId == itemId);
        if (cartItem != null)
        {
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }

        var updatedCart = await GetUserCart(userId);
        await Clients.User(userId.ToString()).SendAsync("ReceiveCartUpdate", updatedCart);
    }

    private async Task<List<CartItem>> GetUserCart(int userId)
    {
        return await _context.CartItems
            .Where(c => c.UserId == userId)
            .Include(c => c.FoodItem)
            .ToListAsync();
    }
}
