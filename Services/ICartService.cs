using System.Threading.Tasks;
using System.Collections.Generic;
using VideoDiningApp.Models;

namespace VideoDiningApp.Services
{
    public interface ICartService
    {
        Task<List<CartItem>> GetCartByUserId(int userId); 
        Task IncreaseQuantity(int userId, int itemId);
        Task DecreaseQuantity(int userId, int itemId);
        Task RemoveItem(int userId, int itemId);
    }
}
