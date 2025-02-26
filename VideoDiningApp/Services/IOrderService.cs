using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoDiningApp.Models;

namespace VideoDiningApp.Services
{
    public interface IOrderService
    {
        Task<List<Order>> GetOrdersAsync();  
        Task<Order> GetOrderByIdAsync(int orderId);  
        Task<List<Order>> GetGroupOrdersAsync(Guid groupOrderId);  
        Task<string> ConfirmDeliveryAsync(int userId, Guid groupOrderId);  
        Task UpdateOrderAsync(Order order); 
        Task<bool> CancelOrderAsync(int orderId, string userEmail);

        void MarkOrderAsPaid(string email, Guid groupOrderId);
    }
}
