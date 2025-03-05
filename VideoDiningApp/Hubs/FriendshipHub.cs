using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace VideoDiningApp.Hubs
{
    public class FriendshipHub : Hub
    {
        public async Task SendFriendRequest(int senderId, int receiverId)
        {
            await Clients.User(receiverId.ToString()).SendAsync("ReceiveFriendRequest", senderId);
        }

        public async Task FriendRequestAccepted(int senderId, int receiverId)
        {
            await Clients.User(senderId.ToString()).SendAsync("FriendRequestAccepted", receiverId);
        }

        public async Task FriendRequestRejected(int senderId, int receiverId)
        {
            await Clients.User(senderId.ToString()).SendAsync("FriendRequestRejected", receiverId);
        }
    }
}
