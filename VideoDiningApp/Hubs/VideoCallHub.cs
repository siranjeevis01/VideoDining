using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace VideoDiningApp.Hubs
{
    public class VideoCallHub : Hub
    {
        public async Task SendCallNotification(int userId, string message)
        {
            await Clients.User(userId.ToString()).SendAsync("ReceiveCallNotification", message);
        }

        public async Task StartCall(int userId, string videoCallUrl)
        {
            await Clients.User(userId.ToString()).SendAsync("StartVideoCall", videoCallUrl);
        }

        public async Task EndCall(int userId, string message)
        {
            await Clients.User(userId.ToString()).SendAsync("EndVideoCall", message);
        }
    }
}
