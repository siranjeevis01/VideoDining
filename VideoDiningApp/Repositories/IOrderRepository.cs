using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoDiningApp.Enums;
using VideoDiningApp.Models;

namespace VideoDiningApp.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrder(Order order);
        Task<IEnumerable<Order>> GetAllOrders();
        Task<Order> GetOrderById(int id);
        Task<IEnumerable<Order>> GetOrdersByUserId(int userId);
        Task<bool> UpdateOrder(int id, Order order);
        Task<bool> DeleteOrder(int id);
        Task<bool> UpdatePaymentStatus(int orderId, PaymentStatus status);
        Task<Order> GetOrderByUserAndGroupId(int userId, Guid groupOrderId); 
        Task<List<Order>> GetOrdersByGroupId(Guid groupOrderId); 
        Task<bool> MarkGroupOrderAsPaid(Guid groupOrderId);
        Task<bool> SavePaymentAsync(Payment payment);
    }
}
