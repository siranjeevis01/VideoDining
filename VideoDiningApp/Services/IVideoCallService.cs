using System;
using System.Threading.Tasks;

namespace VideoDiningApp.Services
{
    public interface IVideoCallService
    {
        Task<bool> RejectCallAsync(int userId, Guid callId);
        Task<bool> EndCallAsync(Guid callId);
        Task HandleUserDisconnectedAsync(int userId);
    }
}
