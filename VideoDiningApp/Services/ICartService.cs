using VideoDiningApp.Models;

namespace VideoDiningApp.Services
{
    public interface ICartService
    {
        Task<IEnumerable<CartItem>> GetCartByGroupOrderId(Guid groupOrderId);
    }
}
