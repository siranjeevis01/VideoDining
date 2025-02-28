using Microsoft.EntityFrameworkCore;
using VideoDiningApp.Data;
using VideoDiningApp.Models;

namespace VideoDiningApp.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;

        public CartService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CartItem>> GetCartByGroupOrderId(Guid groupOrderId)
        {
            return await _context.CartItems
                .Where(c => c.GroupOrderId == groupOrderId)
                .ToListAsync();
        }
    }
}
