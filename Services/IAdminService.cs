using System.Collections.Generic;
using System.Threading.Tasks;
using VideoDiningApp.Models;

namespace VideoDiningApp.Services
{
    public interface IAdminService
    {
        Task<string> LoginAsync(LoginRequest request);
        Task<IEnumerable<User>> GetUsersAsync();
        Task<bool> DeleteUserAsync(int userId);
        Task<List<FoodItem>> GetFoodsAsync();
        Task<FoodItem> GetFoodByIdAsync(int id);
        Task<FoodItem> AddFoodAsync(FoodItem food);
        Task<bool> UpdateFoodAsync(int id, FoodItem food);
        Task<bool> DeleteFoodAsync(int id);
        Task<IEnumerable<Order>> GetOrdersAsync();
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
        Task<List<Payment>> GetAllPayments();
    }
}
