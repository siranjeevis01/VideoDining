using System.Threading.Tasks;
using VideoDiningApp.DTOs;
using VideoDiningApp.Models;

namespace VideoDiningApp.Services
{
    public interface IPaymentService
    {
        Task<string> GenerateAndSendOtpAsync(int orderId, string userEmail);
        Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);

        public string GenerateFakeSignature(int orderId, string otp)
        {
            return $"SIGN_{orderId}_{otp}";
        }

        public bool ConfirmPayment(string razorpaySignature)
        {
            return !string.IsNullOrEmpty(razorpaySignature) && razorpaySignature.StartsWith("SIGN_");
        }
        Task<string> GeneratePaymentLink(int orderId, int userId, decimal amount);
    }

}
