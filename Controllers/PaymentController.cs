using VideoDiningApp.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VideoDiningApp.Data;
using VideoDiningApp.Hubs;
using VideoDiningApp.Models;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly EmailService _emailService;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IOrderService _orderService;

    public PaymentController(ApplicationDbContext context, EmailService emailService, IHubContext<NotificationHub> hubContext, IOrderService orderService)
    {
        _context = context;
        _emailService = emailService;
        _hubContext = hubContext;
        _orderService = orderService;
    }

    [HttpPost("send-links")]
    public async Task<IActionResult> SendPaymentLinks([FromBody] PaymentData paymentData)
    {
        try
        {
            if (paymentData.UserId <= 0 || paymentData.OrderId <= 0)
                return BadRequest(new { message = "Invalid User or Order ID" });

            var order = await _context.Orders
                .Include(o => o.OrderPayments)
                .FirstOrDefaultAsync(o => o.Id == paymentData.OrderId);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            var user = await _context.Users.FindAsync(paymentData.UserId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            string frontendBaseUrl = "http://localhost:3000";

            string userOtp = GenerateOtp();
            var userPayment = order.OrderPayments.FirstOrDefault(p => p.UserId == paymentData.UserId);

            if (userPayment != null)
            {
                userPayment.OtpCode = userOtp;
            }
            else
            {
                var newOrderPayment = new OrderPayment();
                newOrderPayment.Initialize(order, user);
                newOrderPayment.OtpCode = userOtp;
                newOrderPayment.IsPaid = false;   

                order.OrderPayments.Add(newOrderPayment);
            }


            await _context.SaveChangesAsync();

            string paymentLink = $"{frontendBaseUrl}/payment?orderId={paymentData.OrderId}&userId={paymentData.UserId}";

            Console.WriteLine($"Sending payment link email to: {user.Email}");
            Console.WriteLine($"Payment Link: {paymentLink}");
            Console.WriteLine($"OTP: {userOtp}");

            await _emailService.SendEmailAsync(user.Email, "Payment Details",
                $"<p>Click below to complete your payment:</p>" +
                $"<a href='{paymentLink}' target='_blank' style='color: blue; text-decoration: underline;'>Pay Now</a>" +
                $"<p>Use OTP: <strong>{userOtp}</strong> to verify your payment.</p>");

            await _hubContext.Clients.User(user.Email).SendAsync("ReceiveNotification", "Payment link & OTP sent successfully.");

            foreach (var friendPayment in order.OrderPayments.Where(p => !p.IsPaid && p.UserId != paymentData.UserId))
            {
                var friend = await _context.Users.FindAsync(friendPayment.UserId);
                if (friend != null)
                {
                    string friendOtp = GenerateOtp();
                    friendPayment.OtpCode = friendOtp;

                    await _context.SaveChangesAsync();

                    string friendPaymentLink = $"{frontendBaseUrl}/payment?orderId={paymentData.OrderId}&userId={friend.Id}";
                    await _emailService.SendEmailAsync(friend.Email, "Your Payment Link",
                        $"<p>Click below to complete your payment:</p>" +
                        $"<a href='{friendPaymentLink}' target='_blank' style='color: blue; text-decoration: underline;'>Pay Now</a>" +
                        $"<p>Use OTP: <strong>{friendOtp}</strong> to verify your payment.</p>");

                    await _hubContext.Clients.User(friend.Email).SendAsync("ReceiveNotification", "You received a payment link.");
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Payment links sent successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error sending payment links: " + ex.Message });
        }
    }

    [HttpPost("verifyOtp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var order = await _context.Orders
                                  .Include(o => o.OrderPayments)
                                  .FirstOrDefaultAsync(o => o.Id == request.OrderId);

        if (order == null)
            return NotFound(new { message = "Order not found." });

        var payment = order.OrderPayments.FirstOrDefault(p => p.UserId == request.UserId);
        if (payment == null)
            return BadRequest(new { message = "Payment record not found for this user." });

        if (payment.IsPaid)
            return BadRequest(new { message = "User has already completed payment." });

        if (payment.OtpCode != request.Otp)
            return BadRequest(new { message = "Invalid OTP." });

        payment.IsPaid = true;
        await _context.SaveChangesAsync();

        if (order.OrderPayments.All(p => p.IsPaid))
        {
            order.Status = "Confirmed";
            await _context.SaveChangesAsync();
        }

        return Ok(new { message = "Payment successful!" });
    }

    [HttpPost("generateOtp")]
    public async Task<IActionResult> GenerateOtp([FromBody] PaymentData request)
    {
        var order = await _context.Orders
                                  .Include(o => o.OrderPayments)
                                  .FirstOrDefaultAsync(o => o.Id == request.OrderId);
        if (order == null)
            return NotFound(new { message = "Order not found." });

        var userPayment = order.OrderPayments.FirstOrDefault(p => p.UserId == request.UserId);
        if (userPayment == null)
            return NotFound(new { message = "Payment record not found for this user." });

        string otp = GenerateOtp();
        userPayment.OtpCode = otp;
        await _context.SaveChangesAsync();

        return Ok(new { otp });
    }

    private string GenerateOtp()
    {
        Random random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    [HttpPost("confirm-payment")]
    public async Task<IActionResult> ConfirmPayment([FromBody] PaymentData paymentData)
    {
        if (paymentData.UserId <= 0 || paymentData.OrderId <= 0)
            return BadRequest(new { message = "Invalid User or Order ID" });

        bool isConfirmed = await _orderService.ConfirmPayment(paymentData.UserId, paymentData.OrderId);
        if (!isConfirmed)
            return BadRequest(new { message = "Payment confirmation failed" });

        return Ok(new { message = "Payment confirmed successfully!" });
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
    }

    [HttpPost("pay/{orderId}")]
    public async Task<IActionResult> PayOrder(int orderId)
    {
        var userId = GetUserId();

        var success = await _orderService.UpdateOrderPaymentStatus(orderId, userId);

        if (!success)
            return BadRequest("Payment update failed");

        return Ok(new { message = "Payment successful!" });
    }

    [HttpPost("success")]
    public async Task<IActionResult> PaymentSuccess([FromBody] PaymentSuccessRequest request)
    {
        var orderDto = await _orderService.GetOrderById(request.OrderId);
        if (orderDto != null)
        {
            var order = new Order
            {
                Id = orderDto.Id,
                PaymentStatus = orderDto.PaymentStatus,
                Status = orderDto.Status
            };
            await _orderService.UpdateOrder(order);

            var allPaid = await _orderService.CheckAllFriendsPaid(order.Id);
            if (allPaid)
            {
                await _emailService.SendPaymentConfirmationEmail(request.UserEmail, order.Id);
                return Ok("Payment processed successfully.");
            }
        }
        return BadRequest("Payment failed or invalid order.");
    }

}

public class PaymentData
{
    [Required] public int UserId { get; set; }
    [Required] public int OrderId { get; set; }
}

public class VerifyOtpRequest
{
    [Required] public int UserId { get; set; }
    [Required] public int OrderId { get; set; }
    [Required] public string Otp { get; set; }
}

public class PaymentSuccessRequest
{
    public int OrderId { get; set; }
    public string PaymentReference { get; set; }
    public string Status { get; set; }
    public string UserEmail { get; set; }
}
