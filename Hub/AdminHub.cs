using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace VideoDiningApp.Hubs
{
    public class AdminHub : Hub
    {
        public async Task NotifyUsersUpdated()
        {
            await Clients.All.SendAsync("userUpdated");
        }

        public async Task NotifyOrdersUpdated()
        {
            await Clients.All.SendAsync("orderUpdated");
        }

        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
