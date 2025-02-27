using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using VideoDiningApp.Data;
using VideoDiningApp.Enums;
using VideoDiningApp.Hubs;
using VideoDiningApp.Models;
using VideoDiningApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/orders")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IHubContext<OrderHub> _orderHub;
    private readonly IOrderService _orderService;

    public OrderController(AppDbContext context, IEmailService emailService, IHubContext<OrderHub> orderHub, IOrderService orderService)
    {
        _context = context;
        _emailService = emailService;
        _orderHub = orderHub;
        _orderService = orderService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
    {
        var user = await _context.Users
            .Include(u => u.FriendshipsAsUser1).ThenInclude(f => f.User2)
            .Include(u => u.FriendshipsAsUser2).ThenInclude(f => f.User1)
            .FirstOrDefaultAsync(u => u.Id == request.UserId);

        if (user == null)
            return NotFound(new { message = "User not found." });

        var friends = user.FriendshipsAsUser1.Select(f => f.User2)
            .Concat(user.FriendshipsAsUser2.Select(f => f.User1))
            .ToList();

        if (!friends.Any())
            return BadRequest(new { message = "You need at least one friend before ordering food." });

        var validFoodItems = await _context.FoodItems
            .Where(f => request.FoodItems.Contains(f.Id))
            .ToListAsync();

        if (validFoodItems.Count != request.FoodItems.Count)
            return BadRequest(new { message = "One or more food items do not exist." });

        decimal totalAmount = validFoodItems.Sum(f => f.Price);

        Guid groupOrderId = Guid.NewGuid();

        var userOrder = new Order
        {
            UserId = user.Id,
            UserEmail = user.Email,  // Ensure UserEmail is set
            FoodItemsSerialized = JsonConvert.SerializeObject(validFoodItems.Select(f => f.Id).ToList()),
            TotalAmount = totalAmount,
            EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(30),
            GroupOrderId = groupOrderId,
            PaymentStatus = PaymentStatus.PENDING,
            Status = "Pending"
        };

        _context.Orders.Add(userOrder);
        await _context.SaveChangesAsync();

        Console.WriteLine("New GroupOrderId: " + groupOrderId);

        return Ok(new
        {
            message = "Order created successfully!",
            orderId = userOrder.Id,
            userId = userOrder.UserId,
            totalAmount = userOrder.TotalAmount,
            estimatedDeliveryTime = userOrder.EstimatedDeliveryTime,
            groupOrderId = userOrder.GroupOrderId.ToString(),
            foodItems = validFoodItems.Select(f => new { f.Id, f.Name, f.Price })
        });
    }

    [HttpGet("get-order/{orderId}")]
    public async Task<IActionResult> GetOrder(int orderId)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return NotFound(new { message = "Order not found." });

        return Ok(order);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserOrders(int userId)
    {
        var orders = await _context.Orders.Where(o => o.UserId == userId).ToListAsync();
        return orders.Any() ? Ok(orders) : NotFound(new { message = "No orders found." });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetOrderHistory()
    {
        var orders = await _orderService.GetOrdersAsync();
        return Ok(orders);
    }

    [HttpGet("complete-payment")]
    public async Task<IActionResult> CompletePayment([FromQuery] int userId, [FromQuery] Guid groupOrderId)
    {
        var orderDetails = await _context.Orders
            .FirstOrDefaultAsync(o => o.UserId == userId && o.GroupOrderId == groupOrderId);

        if (orderDetails == null)
            return NotFound(new { message = "Order not found." });

        if (orderDetails.PaymentStatus == PaymentStatus.COMPLETED)
            return Ok(new { message = "Payment already completed." });

        orderDetails.PaymentStatus = PaymentStatus.COMPLETED;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Payment successful. Waiting for other group members." });
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var orderToDelete = await _context.Orders.FindAsync(id);
        if (orderToDelete == null)
            return NotFound(new { message = "Order not found." });

        if (orderToDelete.PaymentStatus == PaymentStatus.COMPLETED)
            return BadRequest(new { message = "Paid orders cannot be cancelled." });

        _context.Orders.Remove(orderToDelete);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Order cancelled successfully." });
    }

    [HttpPut("update-status/{orderId}")]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] string status)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
            return NotFound(new { message = "Order not found." });

        order.Status = status;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Order status updated successfully." });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("block-user/{userId}")]
    public async Task<IActionResult> BlockUser(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found." });

        user.IsBlocked = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "User blocked successfully." });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("unblock-user/{userId}")]
    public async Task<IActionResult> UnblockUser(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found." });

        user.IsBlocked = false;
        await _context.SaveChangesAsync();

        return Ok(new { message = "User unblocked successfully." });
    }

    [HttpGet("get-latest-group-order")]
    public async Task<IActionResult> GetLatestGroupOrder()
    {
        var latestOrder = await _context.Orders
            .OrderByDescending(o => o.Id)
            .FirstOrDefaultAsync();

        if (latestOrder == null)
            return NotFound(new { message = "No orders found. Please place an order first." });

        return Ok(new
        {
            groupOrderId = latestOrder.GroupOrderId,
            orderId = latestOrder.Id 
        });
    }
}


