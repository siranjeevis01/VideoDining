using System.Threading.Tasks;

namespace VideoDiningApp.Services
{
    public interface IOtpService
    {
        Task SaveOtpAsync(int orderId, string email, string otp);
        Task<bool> ValidateOtpAsync(int orderId, string email, string otp);

        string GenerateOtp(string email);
        bool ValidateOtp(string email, string otp);
    }
}
