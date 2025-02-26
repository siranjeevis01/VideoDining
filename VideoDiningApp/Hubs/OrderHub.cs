using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace VideoDiningApp.Hubs
{
    public class OrderHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnConnectedAsync();
        }

        public async Task NotifyOrderUpdate(int userId, string message)
        {
            await Clients.Group(userId.ToString()).SendAsync("ReceiveOrderUpdate", message);
        }

        public async Task NotifyVideoCall(int userId, string videoCallUrl)
        {
            await Clients.User(userId.ToString()).SendAsync("StartVideoCall", videoCallUrl);
        }
    }
}
