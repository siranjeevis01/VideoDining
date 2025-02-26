using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VideoDiningApp.Data;
using VideoDiningApp.Models;
using VideoDiningApp.Services;

public class OtpService : IOtpService
{
    private readonly AppDbContext _dbContext;

    public OtpService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveOtpAsync(int orderId, string email, string otp)
    {
        var expiryTime = DateTime.UtcNow.AddMinutes(5);

        var existingOtp = await _dbContext.Otps.FirstOrDefaultAsync(o => o.OrderId == orderId && o.Email == email);
        if (existingOtp != null)
        {
            existingOtp.Code = otp;
            existingOtp.ExpiryTime = expiryTime;
            existingOtp.IsUsed = false;
        }
        else
        {
            var otpEntry = new Otp
            {
                OrderId = orderId,
                Email = email,
                Code = otp,
                ExpiryTime = expiryTime,
                IsUsed = false
            };
            _dbContext.Otps.Add(otpEntry);
        }

        await _dbContext.SaveChangesAsync();
    }

    private static Dictionary<string, string> otpStorage = new Dictionary<string, string>();

    public string GenerateOtp(string email)
    {
        string otp = new Random().Next(100000, 999999).ToString();
        otpStorage[email] = otp;
        Console.WriteLine($"OTP for {email}: {otp}"); // Simulating sending OTP via email
        return otp;
    }

    public bool ValidateOtp(string email, string userOtp)
    {
        return otpStorage.ContainsKey(email) && otpStorage[email] == userOtp;
    }

    public async Task<bool> ValidateOtpAsync(int orderId, string email, string otp)
    {
        var otpEntry = await _dbContext.Otps.FirstOrDefaultAsync(o => o.OrderId == orderId && o.Email == email);

        if (otpEntry == null || otpEntry.IsUsed || otpEntry.ExpiryTime < DateTime.UtcNow || otpEntry.Code.Trim() != otp.Trim())
            return false;

        otpEntry.IsUsed = true;
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
