using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using VideoDiningApp.Enums;
using VideoDiningApp.Models;
using VideoDiningApp.Repositories;
using VideoDiningApp.Services;
using Microsoft.EntityFrameworkCore;
using VideoDiningApp.Data;

public class RazorpayPaymentService : IPaymentService
{
    private readonly IOrderRepository _orderRepository;
    private readonly AppDbContext _dbContext;
    private readonly IEmailService _emailService;
    private static readonly ConcurrentDictionary<int, (string Otp, DateTime Expiry)> _otpStorage = new();

    public RazorpayPaymentService(IOrderRepository orderRepository, IEmailService emailService, AppDbContext dbContext)
    {
        _orderRepository = orderRepository;
        _emailService = emailService;
        _dbContext = dbContext;
    }

    public async Task<string> GenerateAndSendOtpAsync(int orderId, string userEmail)
    {
        string otp = new Random().Next(100000, 999999).ToString();
        _otpStorage[orderId] = (otp, DateTime.UtcNow.AddMinutes(5));

        string subject = "Payment OTP Verification";
        string message = $"Use {otp} to complete your payment for order #{orderId}. This OTP expires in 5 minutes.";

        Console.WriteLine($"Generated OTP: {otp} for Order #{orderId}");
        await _emailService.SendEmailAsync(new List<string> { userEmail }, subject, message);

        return "OTP has been sent successfully.";
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
    {
        var order = await _orderRepository.GetOrderById(request.OrderId);
        if (order == null)
        {
            return CreateFailedResponse("Order not found.");
        }

        if (order.PaymentStatus == PaymentStatus.COMPLETED)
        {
            return CreateFailedResponse("Payment already completed.");
        }

        if (!Guid.TryParse(request.GroupOrderId.ToString(), out Guid groupOrderGuid))
        {
            return CreateFailedResponse("Invalid Group Order ID.");
        }

        var groupOrders = await _orderRepository.GetOrdersByGroupId(groupOrderGuid);
        var pendingOrders = groupOrders.Where(o => o.PaymentStatus != PaymentStatus.COMPLETED).ToList();

        if (pendingOrders.Any())
        {
            foreach (var pendingOrder in pendingOrders)
            {
                await _emailService.SendPaymentLinkAsync(pendingOrder.UserEmail, pendingOrder.Id);
            }

            return new PaymentResponse
            {
                PaymentId = GeneratePaymentId(),
                Status = PaymentStatus.PENDING,
                Amount = order.TotalAmount,
                Timestamp = DateTime.UtcNow,
                Message = "Payment successful for this user, but waiting for the group to complete payment."
            };
        }

        await _orderRepository.MarkGroupOrderAsPaid(groupOrderGuid);
        Console.WriteLine($"Group Order {groupOrderGuid} marked as paid.");

        order.PaymentStatus = PaymentStatus.COMPLETED;
        await _orderRepository.UpdateOrder(order.Id, order);

        // ✅ **Fix: Save the Payment Record in DB**
        var payment = new Payment
        {
            OrderId = order.Id,
            Amount = order.TotalAmount,
            PaymentStatus = PaymentStatus.COMPLETED,
            PaymentMethod = "Razorpay",
            TransactionId = GeneratePaymentId(),
            CreatedAt = DateTime.UtcNow
        };

        await _orderRepository.SavePaymentAsync(payment);
        Console.WriteLine($"Payment record created for Order ID: {order.Id}, Amount: {order.TotalAmount}");

        return new PaymentResponse
        {
            PaymentId = payment.TransactionId,
            Status = PaymentStatus.COMPLETED,
            Amount = order.TotalAmount,
            Timestamp = DateTime.UtcNow,
            Message = "Payment successful. The group order is now fully paid."
        };
    }

    public async Task AutoCancelUnpaidOrders()
    {
        var timeLimit = TimeSpan.FromMinutes(15);
        var thresholdTime = DateTime.UtcNow - timeLimit;

        var unpaidOrders = await _dbContext.Orders
            .Where(o => o.PaymentStatus == PaymentStatus.PENDING && o.CreatedAt < thresholdTime)
            .ToListAsync();

        foreach (var order in unpaidOrders)
        {
            order.PaymentStatus = PaymentStatus.CANCELED;
        }

        await _dbContext.SaveChangesAsync();
    }

    private PaymentResponse CreateFailedResponse(string reason)
    {
        return new PaymentResponse
        {
            PaymentId = null,
            Status = PaymentStatus.FAILED,
            Message = "Payment failed.",
            Reason = reason,
            Timestamp = DateTime.UtcNow
        };
    }
        
    private string GeneratePaymentId()
    {
        return $"pay_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
    }

    public async Task<string> GeneratePaymentLink(int orderId, int userId, decimal amount)
    {
        string localPaymentLink = $"https://localhost:7179/payment/process?orderId={orderId}&userId={userId}&amount={amount}";

        Console.WriteLine($"Generated Local Payment Link: {localPaymentLink}");
        return await Task.FromResult(localPaymentLink);
    }

}
