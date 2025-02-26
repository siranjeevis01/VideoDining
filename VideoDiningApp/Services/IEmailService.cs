using System.Threading.Tasks;

namespace VideoDiningApp.Services
{
    public interface IEmailService
    {
        Task<bool> ValidateOtpAsync(string email, string otp);
        Task<bool> SendEmailAsync(List<string> recipients, string subject, string body);
        Task SendVideoCallInviteAsync(List<string> recipients, string callUrl);
        Task SendPaymentConfirmationEmailAsync(string email, string orderId);
        Task SendOtpEmailAsync(string email, string otp);
        Task SendPaymentLinkAsync(string userEmail, int orderId);
        Task SendCancellationNotificationAsync(string userEmail, int orderId); 
    }
}
