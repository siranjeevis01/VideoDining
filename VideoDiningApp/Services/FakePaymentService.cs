using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using VideoDiningApp.Models;
using VideoDiningApp.Services;
using VideoDiningApp.Enums;

public class FakePaymentService : IPaymentService
{
    private readonly IOrderService _orderService;
    private readonly IFoodService _foodService;

    public FakePaymentService(IOrderService orderService, IFoodService foodService)
    {
        _orderService = orderService;
        _foodService = foodService;
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
    {
        var order = await _orderService.GetOrderByIdAsync(request.OrderId);
        if (order == null)
        {
            return new PaymentResponse
            {
                PaymentId = null,
                Status = PaymentStatus.FAILED,  // ✅ Using enum directly
                Message = "Order not found",
                Timestamp = DateTime.UtcNow
            };
        }

        if (order.PaymentStatus == PaymentStatus.COMPLETED)
        {
            return new PaymentResponse
            {
                PaymentId = null,
                Status = PaymentStatus.FAILED,  // ✅ Using enum directly
                Message = "Payment already completed",
                Timestamp = DateTime.UtcNow
            };
        }

        var foodItems = await _foodService.GetFoodItemsByIdsAsync(order.FoodItems);
        decimal totalAmount = foodItems.Sum(f => f.Price);

        return new PaymentResponse
        {
            PaymentId = $"pay_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Status = PaymentStatus.COMPLETED,  // ✅ Using enum directly
            Amount = totalAmount,
            Timestamp = DateTime.UtcNow,
            Message = "Payment successful!"
        };
    }
    public async Task<string> GeneratePaymentLink(int orderId, int userId, decimal amount)
    {
        return await Task.FromResult($"https://localhost:7179/fake-payment?orderId={orderId}&userId={userId}&amount={amount}");
    }

    public async Task<string> GenerateAndSendOtpAsync(int orderId, string userEmail)
    {
        string otp = new Random().Next(100000, 999999).ToString();
        await Task.Delay(500);
        Console.WriteLine($"OTP {otp} sent to {userEmail} for Order ID: {orderId}");
        return otp;
    }
}
