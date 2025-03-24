using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoDiningApp.Data;
using VideoDiningApp.Models;
using VideoDiningApp.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace VideoDiningApp.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderController> _logger;
        private readonly IEmailService _emailService;

        public OrderController(IOrderService orderService, ApplicationDbContext context, ILogger<OrderController> logger, IEmailService emailService)
        {
            _orderService = orderService;
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }


        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrders();

                var response = orders.Select(o => new
                {
                    orderId = o.Id,
                    status = o.Status,
                    orderItems = o.OrderItems.Select(oi => new
                    {
                        id = oi.Id,
                        foodItemId = oi.FoodItemId,
                        quantity = oi.Quantity,
                        price = oi.Price,
                        name = oi.Name,
                        imageUrl = oi.ImageUrl
                    }).ToList() 
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders");
                return StatusCode(500, new { error = "Internal Server Error", details = ex.Message });
            }
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrder(int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.OrderPayments) 
                    .ThenInclude(op => op.User) 
                    .Include(o => o.OrderFriends)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    return NotFound(new { message = "Order not found" });

                Console.WriteLine($"🔍 Order ID: {order.Id}, Status: {order.Status}");
                Console.WriteLine($"🛒 Items Count: {order.OrderItems.Count}");
                Console.WriteLine($"Order Payments Count: {order.OrderPayments.Count}");
                Console.WriteLine($"Friends Payment Status: {order.OrderPayments.Select(p => new { p.UserId, p.HasPaid })}");

                var response = new
                {
                    orderId = order.Id,
                    status = order.Status,
                    friendsPaymentStatus = order.OrderPayments.Select(op => new
                    {
                        userId = op.UserId,
                        email = op.User?.Email ?? "Unknown",
                        hasPaid = op.HasPaid,
                        name = op.User?.Name ?? "Unknown"
                    }).ToList(),
                    orderItems = order.OrderItems.Select(oi => new
                    {
                        id = oi.Id,
                        foodItemId = oi.FoodItemId,
                        quantity = oi.Quantity,
                        price = oi.Price,
                        name = oi.Name,
                        imageUrl = oi.ImageUrl
                    }).ToList(),
                    expectedDelivery = order.Status == "Confirmed"
                        ? order.ExpectedDeliveryTime?.ToString("yyyy-MM-dd HH:mm:ss")
                        : "TBD"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order {orderId}", orderId);
                return StatusCode(500, new { error = "Internal Server Error", details = ex.Message });
            }
        }

        [HttpPost("create/{userId}")]
        public async Task<IActionResult> CreateOrder(int userId, [FromBody] OrderRequest request)
        {
            Console.WriteLine($"📦 Received Order Request for User {userId}");
            Console.WriteLine($"🛒 Order Items: {request.OrderItems?.Count}, 👥 Friends: {request.Friends?.Count}");

            if (userId <= 0)
                return BadRequest(new { message = "Invalid user ID." });

            if (request.OrderItems == null || request.OrderItems.Count == 0)
                return BadRequest(new { message = "Cart cannot be empty!" });

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                return NotFound(new { message = "User not found." });

            var order = await _orderService.CreateOrderAsync(userId, request.OrderItems, request.Friends);

            if (order == null)
                return BadRequest(new { message = "Error creating order." });

            return CreatedAtAction(nameof(GetOrders), new { userId = userId }, new { orderId = order.Id });
        }

        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetOrderHistory(int userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            if (!orders.Any())
                return NotFound(new { message = "No orders found." });

            return Ok(orders);
        }

        [HttpGet("status/{orderId}")]
        public async Task<IActionResult> GetOrderStatus(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderPayments)
                .ThenInclude(op => op.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            var paidUsers = order.OrderPayments?
                .Where(op => op.HasPaid && op.User != null)
                .Select(op => new PaymentUser { Id = op.User.Id, Email = op.User.Email })
                .ToList() ?? new List<PaymentUser>();

            var unpaidUsers = order.OrderPayments?
                .Where(op => !op.HasPaid && op.User != null)
                .Select(op => new PaymentUser { Id = op.User.Id, Email = op.User.Email })
                .ToList() ?? new List<PaymentUser>();

            return Ok(new
            {
                order.Status,
                paidUsers,
                unpaidUsers
            });
        }

        [HttpPost("update-status/{orderId}")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound(new { message = "Order not found" });

            if (status != "Pending" && status != "On the way" && status != "Reached" && status != "Delivered")
                return BadRequest(new { message = "Invalid status update" });

            order.Status = status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order status updated successfully", order });
        }

        [HttpPost("mark-delivered/{orderId}/{userId}")]
        public async Task<IActionResult> MarkOrderAsDelivered(int orderId, int userId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderPayments)
                .FirstOrDefaultAsync(o => o.Id == orderId); 

            if (order == null)
                return NotFound(new { message = "Order not found" });

            var userPayment = order.OrderPayments.FirstOrDefault(op => op.UserId == userId);
            if (userPayment == null)
                return NotFound(new { message = "User's order payment record not found" });

            userPayment.IsDelivered = true;
            await _context.SaveChangesAsync();

            bool allDelivered = order.OrderPayments.All(op => op.IsDelivered);
            if (allDelivered)
            {
                order.Status = "Delivered";
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Order marked as delivered", allDelivered });
        }

        [HttpPost("remind/{orderId}")]
        public async Task<IActionResult> SendPaymentReminder(int orderId)
        {
            var order = await _context.Orders.Include(o => o.OrderPayments)
                                             .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            var unpaidFriends = await _context.OrderPayments
                .Where(p => p.OrderId == order.Id && !p.HasPaid)
                .Include(p => p.User) 
                .ToListAsync();

            if (unpaidFriends == null || !unpaidFriends.Any())
            {
                return BadRequest(new { message = "All friends have already paid." });
            }

            foreach (var friend in unpaidFriends)
            {
                if (friend.User != null) 
                {
                    await _emailService.SendEmailAsync(friend.User.Email, "Payment Reminder", $"Hi {friend.User.Name}, Please complete your payment (ID: {order.Id}).");
                }
            }

            return Ok(new { message = "Payment reminders sent successfully." });
        }

        [HttpDelete("cancel/{orderId}/{userId}")]
        public async Task<IActionResult> CancelOrder(int orderId, int userId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderPayments)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            var userPayment = order.OrderPayments.FirstOrDefault(op => op.UserId == userId);
            if (userPayment == null)
                return NotFound(new { message = "User order not found" });

            if (userPayment.HasPaid)
                return BadRequest(new { message = "Cannot cancel a paid order" });

            _context.OrderPayments.Remove(userPayment);
            await _context.SaveChangesAsync();

            bool allCancelled = !order.OrderPayments.Any();
            if (allCancelled)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Order canceled successfully", allCancelled });
        }
    }
}

public class PaymentUser
{
    public int Id { get; set; }
    public string Email { get; set; }
}

public class OrderRequest
{
    public List<OrderItemRequest> OrderItems { get; set; }
    public List<int> Friends { get; set; } 
}

public class OrderItemRequest
{
    public int FoodItemId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string Name { get; set; } 
    public string ImageUrl { get; set; } 
}

public class UserPaymentStatus
{
    public int UserId { get; set; }
    public string Name { get; set; }
}

public class UserRequest
{
    public string Email { get; set; }
}
