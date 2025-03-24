using System.Collections.Generic;
using System.Threading.Tasks;
using VideoDiningApp.Models;

namespace VideoDiningApp.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
    }
}
