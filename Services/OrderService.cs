using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VideoDiningApp.Models;
using VideoDiningApp.Data;
using Microsoft.AspNetCore.SignalR;
using VideoDiningApp.Hubs;
using VideoDiningApp.Services;
using System.Security.Cryptography;

namespace VideoDiningApp.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IEmailService _emailService;
        private readonly IVideoCallService _videoCallService;

        public OrderService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext, IEmailService emailService, IVideoCallService videoCallService)
        {
            _context = context;
            _hubContext = hubContext;
            _emailService = emailService;
            _videoCallService = videoCallService;
        }

        public async Task<IEnumerable<OrderPayment>> GetOrderPaymentsAsync(int orderId)
        {
            return await _context.OrderPayments
                .Where(op => op.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<Order> CreateOrderAsync(int userId, List<OrderItemRequest> orderItems, List<int> friends)
        {
            if (orderItems == null || orderItems.Count == 0)
                throw new ArgumentException("Order items cannot be empty.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new ArgumentException("Invalid user ID.");

            decimal totalAmount = orderItems.Sum(item => item.Price * item.Quantity);

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var order = new Order
                    {
                        UserId = userId,
                        User = user,
                        OrderItems = orderItems.Select(item => new OrderItem
                        {
                            Name = item.Name,
                            Price = item.Price,
                            Quantity = item.Quantity,
                            ImageUrl = item.ImageUrl
                        }).ToList(),
                        TotalAmount = totalAmount,
                        Status = "Pending",
                        ExpectedDeliveryTime = DateTime.UtcNow.AddMinutes(30)
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    var friendPayments = friends.Select(friendId =>
                    {
                        var friend = _context.Users.FirstOrDefault(f => f.Id == friendId); 
                        var orderPayment = new OrderPayment();
                        orderPayment.Initialize(order, friend);
                        return orderPayment;
                    }).ToList();

                    _context.OrderPayments.AddRange(friendPayments);
                    await _context.SaveChangesAsync();

                    foreach (var friendId in friends)
                    {
                        var friend = await _context.Users.FindAsync(friendId);
                        if (friend != null && !string.IsNullOrEmpty(friend.Email))
                        {
                            string email = friend.Email;
                            await _hubContext.Clients.User(email).SendAsync("ReceiveNotification", "You've been added to a group order. Complete your payment.");
                        }
                    }

                    await transaction.CommitAsync();

                    return order;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<bool> ConfirmPayment(int userId, int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderPayments)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return false;

            var payment = order.OrderPayments.FirstOrDefault(p => p.UserId == userId);
            if (payment == null) return false;
            payment.IsPaid = true;
            await _context.SaveChangesAsync();
            await _hubContext.Clients.Group(orderId.ToString()).SendAsync("ReceiveNotification", $"User {userId} has completed payment.");

            bool allPaid = order.OrderPayments.All(p => p.IsPaid);

            if (allPaid)
            {
                order.Status = "Confirmed";
                order.ExpectedDeliveryTime = DateTime.UtcNow.AddMinutes(30); 
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group(orderId.ToString()).SendAsync("ReceiveNotification", "All payments received! Order is confirmed.");

                var emails = order.OrderPayments.Select(p => p.User.Email).ToList();
                foreach (var email in emails)
                {
                    await _emailService.SendEmailAsync(email, "Order Confirmed", $"Your order is confirmed! Estimated delivery: {order.ExpectedDeliveryTime}.");
                }
            }

            return true;
        }   

        public async Task<bool> UpdateOrderPaymentStatus(int orderId, int userId)
        {
            var orderPayment = await _context.OrderPayments
                .FirstOrDefaultAsync(op => op.OrderId == orderId && op.UserId == userId);

            if (orderPayment == null)
                return false;

            orderPayment.IsPaid = true;
            _context.OrderPayments.Update(orderPayment);
            await _context.SaveChangesAsync();

            var allPaid = await _context.OrderPayments
                .Where(op => op.OrderId == orderId)
                .AllAsync(op => op.IsPaid);

            if (allPaid)
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order != null)
                {
                    order.Status = "Confirmed";
                    order.ExpectedDeliveryTime = DateTime.UtcNow.AddMinutes(30); 
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();

                    await _emailService.SendEmailAsync(order.User.Email, "Order Confirmed", $"Your order is confirmed! Estimated delivery: {order.ExpectedDeliveryTime}.");

                    foreach (var friendPayment in order.OrderPayments)
                    {
                        var friend = await _context.Users.FindAsync(friendPayment.UserId);
                        if (friend != null && !string.IsNullOrEmpty(friend.Email))
                        {
                            await _emailService.SendEmailAsync(friend.Email, "Order Confirmed", $"Your group order is confirmed! Estimated delivery: {order.ExpectedDeliveryTime}.");
                        }
                    }
                }
            }

            return true;
        }

        public async Task<bool> UpdateOrderStatusAfterPayment(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderPayments)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return false;

            var allPaid = order.OrderPayments.All(p => p.IsPaid);
            if (allPaid)
            {
                order.Status = "Ready";  
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group(orderId.ToString()).SendAsync("ReceiveNotification", "All payments received! Order is ready.");

                await _videoCallService.StartCallForOrderAsync(orderId);

                return true;
            }

            return false;
        }

        private string GenerateOtp()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[16];
                rng.GetBytes(randomBytes);
                return BitConverter.ToString(randomBytes).Replace("-", "").Substring(0, 6); 
            }
        }

        public async Task<bool> UpdateOrder(Order order)
        {
            if (order == null) return false; 

            _context.Orders.Update(order);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Order> GetGroupCheckout(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new ArgumentException("Order not found.");

            return order;
        }

        public async Task<List<OrderDto>> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderPayments)
                .ThenInclude(op => op.User)
                .Include(o => o.OrderItems)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    Status = o.Status,
                    OrderPayments = o.OrderPayments.Select(op => new OrderPaymentDto
                    {
                        UserId = op.User.Id,
                        Email = op.User.Email,
                        IsPaid = op.IsPaid
                    }).ToList(),
                    OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        Name = oi.Name,
                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList(),
                    ExpectedDeliveryTime = o.ExpectedDeliveryTime
                })
                .ToListAsync();

            return orders;
        }

        public async Task<OrderDto?> GetOrderById(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderPayments)
                .ThenInclude(op => op.User)
                .Include(o => o.OrderItems)
                .Where(o => o.Id == orderId)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    Status = o.Status,
                    OrderPayments = o.OrderPayments.Select(p => new OrderPaymentDto
                    {
                        UserId = p.User.Id,
                        Email = p.User.Email,
                        IsPaid = p.IsPaid
                    }).ToList(),
                    OrderItems = o.OrderItems.Select(i => new OrderItemDto
                    {
                        Name = i.Name,
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList(),
                    ExpectedDeliveryTime = o.ExpectedDeliveryTime
                })
                .FirstOrDefaultAsync();

            return order;
        }

        public async Task<bool> CheckAllFriendsPaid(int orderId)
        {
            var order = await _context.Orders.Include(o => o.OrderPayments)
                                              .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order != null)
            {
                return order.OrderPayments.All(p => p.IsPaid);
            }

            return false;
        }

        public async Task<string> GetOrderStatusAsync(int orderId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return null;

            return order.Status; 
        }

        public async Task<bool> TriggerVideoCallIfAllOrdersDelivered(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderPayments)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return false;

            var allPaid = order.OrderPayments.All(p => p.IsPaid);
            if (!allPaid) return false;

            await _hubContext.Clients.Group(orderId.ToString()).SendAsync("StartVideoCall", $"Your food has arrived! Join the video call to eat together.");

            await _videoCallService.StartCallForOrderAsync(orderId);    

            return true;
        }
    }
}

public class OrderDto
{
    public int Id { get; set; }
    public string Status { get; set; }
    public List<OrderPaymentDto> OrderPayments { get; set; }
    public List<OrderItemDto> OrderItems { get; set; }
    public DateTime? ExpectedDeliveryTime { get; set; }
    public string PaymentStatus { get; set; }
}

public class OrderPaymentDto
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public bool IsPaid { get; set; }
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int FoodItemId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }
}
