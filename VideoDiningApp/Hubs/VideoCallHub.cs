using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using VideoDiningApp.Services;
using Microsoft.Extensions.Logging;

namespace VideoDiningApp.Hubs
{
    public class VideoCallHub : Hub
    {
        private readonly IVideoCallService _videoCallService;
        private readonly ILogger<VideoCallHub> _logger;

        public VideoCallHub(IVideoCallService videoCallService, ILogger<VideoCallHub> logger)
        {
            _videoCallService = videoCallService;
            _logger = logger;
        }

        public async Task SendCallNotification(int userId, string message)
        {
            await Clients.User(userId.ToString()).SendAsync("ReceiveCallNotification", message);
        }

        public async Task StartCall(int userId, string videoCallUrl)
        {
            await Clients.User(userId.ToString()).SendAsync("StartVideoCall", videoCallUrl);
        }

        public async Task EndCall(Guid callId, string message)
        {
            var success = await _videoCallService.EndCallAsync(callId);
            if (success)
            {
                await Clients.All.SendAsync("EndVideoCall", message);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            int userId = GetUserIdFromContext();
            _logger.LogInformation($"User {userId} disconnected.");

            await _videoCallService.HandleUserDisconnectedAsync(userId);
            await Clients.All.SendAsync("UserDisconnected", userId);

            await base.OnDisconnectedAsync(exception);
        }

        private int GetUserIdFromContext()
        {
            return int.TryParse(Context.UserIdentifier, out int userId) ? userId : 0;
        }
    }
}
