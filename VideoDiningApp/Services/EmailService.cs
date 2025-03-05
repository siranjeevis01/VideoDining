using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VideoDiningApp.Data;

namespace VideoDiningApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context; 

        public EmailService(IConfiguration configuration, AppDbContext context) 
        {
            _configuration = configuration;
            _context = context;
        }

        private async Task SendEmail(string recipient, string subject, string message)
        {
            Console.WriteLine($"Sending email to {recipient}: {subject} - {message}");
            await Task.CompletedTask; 
        }

        public async Task<bool> ValidateOtpAsync(string email, string otp)
        {
            var storedOtp = await _context.Otps.FirstOrDefaultAsync(o => o.Email == email && o.Code == otp);
            return storedOtp != null;
        }

        public async Task<bool> SendEmailAsync(List<string> recipients, string subject, string body)
        {
            if (recipients == null || recipients.Count == 0)
                return false; 

            try
            {
                foreach (var recipient in recipients)
                {
                    await SendEmailToSingleRecipientAsync(recipient, subject, body);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }

        private async Task SendEmailToSingleRecipientAsync(string to, string subject, string body)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:Port"] ?? "587"); 
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];

                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                    throw new Exception("SMTP configuration is missing.");

                using (var smtpClient = new SmtpClient(smtpServer))
                {
                    smtpClient.Port = smtpPort;
                    smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(to);
                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
                throw;
            }
        }

        public async Task SendVideoCallInviteAsync(List<string> recipients, string callUrl)
        {
            string subject = "Your Video Dining Call is Ready!";
            string body = $"Your food has arrived! Join your friends for a video dining experience: <a href='{callUrl}'>Join Now</a>";

            await SendEmailAsync(recipients, subject, body);
        }

        public async Task SendPaymentConfirmationEmailAsync(string email, string orderId)
        {
            try
            {
                var smtpClient = new SmtpClient(_configuration["Email:SmtpServer"])
                {
                    Port = int.Parse(_configuration["Email:Port"]),
                    Credentials = new NetworkCredential(_configuration["Email:Username"], _configuration["Email:Password"]),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration["Email:From"]),
                    Subject = "Payment Confirmation",
                    Body = $"Your payment for order {orderId} was successful.",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Email send failed: " + ex.Message);
            }
        }

        public async Task SendOtpEmailAsync(string email, string otp)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:Port"]);
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];

                using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail),
                        Subject = "Your OTP for Payment",
                        Body = $"<p>Your OTP for order verification is: <b>{otp}</b></p>",
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(email);
                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OTP Email sending failed: {ex.Message}");
                throw;
            }
        }

        public async Task SendPaymentLinkAsync(string userEmail, int orderId)
        {
            string paymentLink = $"{_configuration["PaymentSettings:BaseUrl"]}/pay/{orderId}";
            string subject = "Complete Your Payment";
            string body = $"Please complete your payment for order #{orderId}. Use the following link: <a href='{paymentLink}'>Pay Now</a>";

            await SendEmailAsync(new List<string> { userEmail }, subject, body);
        }

        public async Task SendPaymentLinkEmail(int userId, string paymentLink)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                string subject = "Complete Your Payment";
                string body = $"Click here to pay for your order: {paymentLink}";
                await SendEmail(user.Email, subject, body);
            }
        }

        public async Task SendOrderConfirmationEmail(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Participants)
                .ThenInclude(p => p.User)  
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                Console.WriteLine($"Order not found: {orderId}");
                return;
            }

            string subject = "Order Confirmation";
            string message = $"Your order {order.Id} has been successfully placed.";

            foreach (var participant in order.Participants)
            {
                string userEmail = participant.User?.Email; 
                if (!string.IsNullOrEmpty(userEmail))
                {
                    await SendEmail(userEmail, subject, message);
                }
            }
        }

        public async Task SendCancellationNotificationAsync(string userEmail, int orderId)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:Port"] ?? "587"); 
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];

                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                    throw new Exception("SMTP configuration is missing.");

                using (var smtpClient = new SmtpClient(smtpServer))
                {
                    smtpClient.Port = smtpPort;
                    smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail),
                        Subject = "Order Cancellation Notification",
                        Body = $"Your order with ID {orderId} has been canceled.",
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(userEmail);
                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cancellation email sending failed: {ex.Message}");
                throw;
            }
        }
    }
}
