using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VideoDiningApp.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            string senderEmail = _config["EmailSettings:SenderEmail"];
            string senderPassword = _config["EmailSettings:SenderPassword"];

            var smtpClient = new SmtpClient(_config["EmailSettings:SmtpServer"])
            {
                Port = int.Parse(_config["EmailSettings:Port"]),
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = bool.Parse(_config["EmailSettings:EnableSSL"])
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail); 
            await smtpClient.SendMailAsync(mailMessage);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Email Error: {ex.Message}");
            return false;
        }
    }

    public async Task SendOrderConfirmationEmail(string userEmail, string orderId, DateTime estimatedDelivery)
    {
        string subject = "Your Order is Confirmed!";
        string body = $@"
        <h3>Your order has been confirmed!</h3>
        <p>Order ID: {orderId}</p>
        <p>Estimated Delivery Time: {estimatedDelivery}</p>
        <p>We will notify you once your food arrives. Enjoy!</p>
    ";

        await SendEmailAsync(userEmail, subject, body);
    }

    public async Task SendPaymentConfirmationEmail(string email, int orderId)
    {
        Console.WriteLine($"Sending payment confirmation email to {email} for Order {orderId}");
        await Task.CompletedTask;
    }
}
