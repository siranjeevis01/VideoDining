using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoDiningApp.Data;
using VideoDiningApp.Hubs;
using VideoDiningApp.Models;

[Route("api/cart")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHubContext<OrderHub> _hubContext;

    public CartController(AppDbContext context, IHubContext<OrderHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] CartItemDto cartItem)
    {
        var food = await _context.FoodItems.FindAsync(cartItem.FoodItemId);  
        if (food == null)
            return NotFound(new { message = "Food item not found." });

        var cart = await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == cartItem.UserId);
        if (cart == null)
        {
            cart = new Cart { UserId = cartItem.UserId, Items = new List<CartItem>() };
            _context.Carts.Add(cart);
        }

        cart.Items.Add(new CartItem { FoodItemId = food.Id, Quantity = cartItem.Quantity });  
        await _context.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("CartUpdated", cart.UserId);

        return Ok(cart);
    }

    [HttpDelete("remove/{userId}/{foodItemId}")]
    public async Task<IActionResult> RemoveFromCart(int userId, int foodItemId)  
    {
        var cart = await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null || !cart.Items.Any(i => i.FoodItemId == foodItemId))  
            return NotFound(new { message = "Item not found in cart." });

        var itemToRemove = cart.Items.FirstOrDefault(i => i.FoodItemId == foodItemId);  
        if (itemToRemove != null)
        {
            cart.Items.Remove(itemToRemove); 
        }

        await _context.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("CartUpdated", userId);

        return Ok(cart);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetCart(int userId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.FoodItem)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
            return Ok(new { Items = new List<CartItem>() });

        return Ok(cart);
    }
}

public class CartItemDto
{
    public int UserId { get; set; }
    public int FoodItemId { get; set; }  
    public int Quantity { get; set; }
}
