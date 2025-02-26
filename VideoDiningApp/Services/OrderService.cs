using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoDiningApp.Models;
using VideoDiningApp.Repositories;
using Microsoft.Extensions.Logging;
using VideoDiningApp.Enums;
using VideoDiningApp.Data;

namespace VideoDiningApp.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IOrderRepository _orderRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(AppDbContext context, IOrderRepository orderRepository, IEmailService emailService, ILogger<OrderService> logger)
        {
            _context = context;
            _orderRepository = orderRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<string> ConfirmDeliveryAsync(int userId, Guid groupOrderId)
        {
            try
            {
                var orders = await _orderRepository.GetOrdersByGroupId(groupOrderId);
                if (orders == null || orders.Count == 0)
                {
                    return "Group order not found.";
                }

                var allPaid = orders.All(o => o.PaymentStatus == PaymentStatus.COMPLETED);
                if (!allPaid)
                {
                    return "Payment not completed. Please complete the payment before confirming delivery.";
                }

                var userOrder = orders.FirstOrDefault(o => o.UserId == userId);
                if (userOrder == null)
                {
                    return "User order not found.";
                }

                userOrder.IsDelivered = true;
                bool updated = await _orderRepository.UpdateOrder(userOrder.Id, userOrder);
                if (!updated)
                {
                    return "Failed to update order delivery status.";
                }

                if (orders.All(o => o.IsDelivered))
                {
                    return "All group orders delivered. Delivery confirmation successful.";
                }

                return "Order marked as delivered. Waiting for others.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error confirming delivery for GroupOrderId {groupOrderId}: {ex.Message}");
                return "An error occurred while confirming the delivery.";
            }
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderById(orderId);
            if (order == null)
                _logger.LogWarning($"Order {orderId} not found.");

            return order;
        }

        public async Task<List<Order>> GetOrdersAsync()
        {
            var orders = await _orderRepository.GetAllOrders();
            return orders.ToList(); 
        }

        public async Task UpdateOrderAsync(Order order)
        {
            bool updated = await _orderRepository.UpdateOrder(order.Id, order);
            if (!updated)
                _logger.LogError($"Failed to update Order {order.Id}");
        }

        public async Task<List<Order>> GetGroupOrdersAsync(Guid groupOrderId)
        {
            return await _orderRepository.GetOrdersByGroupId(groupOrderId);
        }

        public async Task<bool> CancelOrderAsync(int orderId, string userEmail)
        {
            var order = await _orderRepository.GetOrderById(orderId);
            if (order == null || order.UserEmail != userEmail)
            {
                return false;
            }

            order.PaymentStatus = PaymentStatus.CANCELED;
            await _orderRepository.UpdateOrder(order.Id, order);

            await _emailService.SendCancellationNotificationAsync(userEmail, orderId);

            return true;
        }
        public decimal CalculateTotalAmount(List<OrderItem> items, int friendsCount)
        {
            if (items == null || !items.Any())
                return 0;

            decimal subtotal = items.Sum(item => item.Price * item.Quantity);

            int totalPeople = friendsCount + 1;
            decimal totalAmount = subtotal * totalPeople;

            return totalAmount;
        }

        public void MarkOrderAsPaid(string email, Guid groupOrderId)
        {
            var orders = _context.Orders
                .Where(o => o.GroupOrderId == groupOrderId && o.UserEmail == email)
                .ToList();

            if (!orders.Any())
            {
                _logger.LogWarning($"No orders found for GroupOrderId: {groupOrderId} and Email: {email}");
                return;
            }

            foreach (var order in orders)
            {
                order.PaymentStatus = PaymentStatus.COMPLETED;
            }

            _context.SaveChanges();
            _logger.LogInformation($"Orders marked as paid for GroupOrderId: {groupOrderId}, Email: {email}");
        }
    }
}
