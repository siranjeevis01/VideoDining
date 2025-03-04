﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using VideoDiningApp.Services;
using VideoDiningApp.Models;
using Microsoft.EntityFrameworkCore;
using VideoDiningApp.Data;
using VideoDiningApp.Enums;
using VideoDiningApp.DTOs;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IOtpService _otpService;
    private readonly IPaymentService _razorpayService;
    private readonly IOrderService _orderService;
    private readonly IEmailService _emailService;
    private readonly ILogger<PaymentController> _logger;
    private readonly AppDbContext _dbContext;

    public PaymentController(
        IOtpService otpService,
        IPaymentService razorpayService,
        IOrderService orderService,
        IEmailService emailService,
        ILogger<PaymentController> logger,
        AppDbContext dbContext)
    {
        _otpService = otpService;
        _razorpayService = razorpayService;
        _orderService = orderService;
        _emailService = emailService;
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] OtpRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || request.GroupOrderId == Guid.Empty)
            return BadRequest(new { message = "Invalid request!" });

        string otp = _otpService.GenerateOtp(request.Email);
        string razorpaySignature = _razorpayService.GenerateFakeSignature(request.GroupOrderId.GetHashCode(), otp);

        string subject = "Your Payment OTP";
        string message = $"Your OTP for Group Order {request.GroupOrderId} is: {otp}. Do not share it with anyone.";

        bool emailSent = await _emailService.SendEmailAsync(new List<string> { request.Email }, subject, message);
        if (!emailSent)
            return StatusCode(500, new { message = "Failed to send OTP. Try again." });

        return Ok(new { message = "OTP sent successfully.", razorpaySignature });
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] OtpVerifyRequest request)
    {
        _logger.LogInformation($"Verifying OTP for {request.Email} (Group OrderId {request.GroupOrderId})");

        if (string.IsNullOrEmpty(request.RazorpaySignature))
            return BadRequest(new { message = "Missing Razorpay signature." });

        bool isOtpValid = _otpService.ValidateOtp(request.Email, request.Otp);
        if (!isOtpValid)
            return BadRequest(new { message = "Invalid OTP!" });

        bool paymentConfirmed = _razorpayService.ConfirmPayment(request.RazorpaySignature);
        if (!paymentConfirmed)
            return BadRequest(new { message = "Payment verification failed!" });

        _orderService.MarkOrderAsPaid(request.Email, request.GroupOrderId);

        return Ok(new { message = "OTP verified successfully!" });
    }

    [HttpPost("process-payment")]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
    {
        _logger.LogInformation($"Processing payment: {JsonConvert.SerializeObject(request)}");

        if (request.OrderId <= 0 || request.GroupOrderId == Guid.Empty || string.IsNullOrEmpty(request.PaymentDetails) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Otp))
            return BadRequest(new { message = "Invalid payment details." });

        // ✅ Validate OTP before proceeding with payment
        bool isOtpValid = _otpService.ValidateOtp(request.Email, request.Otp);
        if (!isOtpValid)
        {
            _logger.LogError($"Invalid or expired OTP for user: {request.Email}, GroupOrderId: {request.GroupOrderId}");
            return BadRequest(new { message = "Invalid or expired OTP. Please verify OTP before making payment." });
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // ✅ Ensure the payment isn't already recorded
            var existingPayment = await _dbContext.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.GroupOrderId == request.GroupOrderId && p.UserEmail == request.Email);

            if (existingPayment != null && existingPayment.Status == "SUCCESS")
            {
                _logger.LogWarning($"Payment already processed for user: {request.Email}, GroupOrderId: {request.GroupOrderId}");
                return BadRequest(new { message = "Payment already processed for this user." });
            }

            var order = await _dbContext.Orders
                .FirstOrDefaultAsync(o => o.GroupOrderId == request.GroupOrderId && o.Id == request.OrderId);

            if (order == null)
            {
                _logger.LogError($"Order not found for GroupOrderId: {request.GroupOrderId}, OrderId: {request.OrderId}");
                return NotFound(new { message = "Order not found for this Group Order." });
            }

            var orderItems = await _dbContext.OrderItems
                .Where(i => i.GroupOrderId == request.GroupOrderId && i.UserEmail == request.Email)
                .ToListAsync();

            if (!orderItems.Any())
            {
                _logger.LogError($"No order items found for user: {request.Email}, GroupOrderId: {request.GroupOrderId}");
                return NotFound(new { message = "No order items found for this user." });
            }

            decimal amountToPay = orderItems.Sum(i => i.Price * i.Quantity);

            // ✅ Store payment details in the database
            var payment = new Payment
            {
                OrderId = request.OrderId,
                GroupOrderId = request.GroupOrderId,
                UserEmail = request.Email,
                PaymentId = $"FAKE_{Guid.NewGuid()}",
                Amount = amountToPay,
                Status = "SUCCESS",  // Fake successful payment
                RazorpaySignature = request.RazorpaySignature,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Payments.Add(payment);
            order.PaymentStatus = PaymentStatus.COMPLETED;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            // ✅ Send a payment confirmation email
            string subject = "Payment Confirmation";
            string message = $"Your payment of ${amountToPay} for Group Order {request.GroupOrderId} was successful.";
            await _emailService.SendEmailAsync(new List<string> { request.Email }, subject, message);

            return Ok(new { message = "Payment successful!", paymentId = payment.PaymentId, amount = amountToPay });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment.");
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Payment processing failed due to an internal error." });
        }
    }

    [HttpPost("cancel-order")]
    public async Task<IActionResult> CancelOrderItem([FromBody] CancelOrderRequest cancelRequest)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var orderItems = await _dbContext.OrderItems
                .Where(i => i.GroupOrderId == cancelRequest.GroupOrderId && i.UserEmail == cancelRequest.UserEmail) // ✅ Use UserEmail
                .ToListAsync();

            if (!orderItems.Any())
                return BadRequest(new { message = "No items found for this user." });

            var existingPayment = await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.GroupOrderId == cancelRequest.GroupOrderId && p.UserEmail == cancelRequest.UserEmail);

            if (existingPayment != null)
            {
                _logger.LogInformation($"Refunding payment for {cancelRequest.UserEmail} (Group OrderId: {cancelRequest.GroupOrderId})");
                existingPayment.Status = "REFUNDED";
            }

            _dbContext.OrderItems.RemoveRange(orderItems);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
            return Ok(new { message = "Order cancelled for user." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment.");
            Console.WriteLine(ex);  
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Payment processing failed due to an internal error." });
        }
    }

    [HttpPost("send-payment-links")]
    public async Task<IActionResult> SendPaymentLinks([FromBody] PaymentLinkRequest request)
    {
        if (request.GroupOrderId == Guid.Empty)
            return BadRequest(new { message = "Invalid Group Order ID." });

        var orders = await _dbContext.OrderItems
            .Where(i => i.GroupOrderId == request.GroupOrderId)
            .GroupBy(i => i.UserEmail)
            .ToDictionaryAsync(g => g.Key, g => g.Select(i => new { i.FoodItem, i.Quantity, i.Price }).ToList());

        foreach (var user in orders)
        {
            string email = user.Key;
            var items = user.Value;

            decimal totalAmount = items.Sum(i => i.Price * i.Quantity);
            string itemDetails = string.Join("\n", items.Select(i => $"{i.FoodItem} x {i.Quantity} - ${i.Price * i.Quantity}"));

            // Localhost payment & cancel links
            string paymentLink = $"https://localhost:7179/pay?groupOrderId={request.GroupOrderId}&email={email}";
            string cancelLink = $"https://localhost:7179/cancel?groupOrderId={request.GroupOrderId}&email={email}";

            string emailBody = $"Hello,\n\nYou ordered:\n{itemDetails}\n\nTotal: ${totalAmount}\n\nPay here: {paymentLink}\nCancel: {cancelLink}";

            await _emailService.SendEmailAsync(new List<string> { email }, "Complete Your Payment", emailBody);
        }

        return Ok(new { message = "Payment links sent successfully." });
    }

    [HttpPost("confirm-payment")]
    public async Task<IActionResult> ConfirmPayment([FromBody] PaymentConfirmationRequest request)
    {
        var order = await _dbContext.Orders.Include(o => o.Participants)
                                          .FirstOrDefaultAsync(o => o.Id == request.OrderId);

        if (order == null) return NotFound("Order not found");

        var participant = order.Participants.FirstOrDefault(p => p.UserId == request.UserId);
        if (participant == null) return BadRequest("User not found in order");

        participant.PaymentStatus = "Paid";
        await _dbContext.SaveChangesAsync();

        // Check if all friends have paid
        if (order.Participants.All(p => p.PaymentStatus == "Paid"))
        {
            // Send confirmation email
            await _emailService.SendOrderConfirmationEmail(order.Id);
        }

        return Ok("Payment confirmed");
    }

}
