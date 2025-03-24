using System.Collections.Generic;
using System.Threading.Tasks;
using VideoDiningApp.Models;

namespace VideoDiningApp.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(int userId, List<OrderItemRequest> orderItems, List<int> friends);
        Task<Order> GetGroupCheckout(int orderId);
        Task<string> GetOrderStatusAsync(int orderId);
        Task<bool> UpdateOrderPaymentStatus(int orderId, int userId);
        Task<List<OrderDto>> GetAllOrders();
        Task<OrderDto?> GetOrderById(int orderId);
        Task<bool> UpdateOrder(Order order);
        Task<bool> CheckAllFriendsPaid(int orderId);
        Task<bool> ConfirmPayment(int userId, int orderId);
        Task<IEnumerable<OrderPayment>> GetOrderPaymentsAsync(int orderId);
    }
}
