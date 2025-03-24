using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class VideoCallHub : Hub
{
    private static ConcurrentDictionary<string, HashSet<string>> ActiveUsers = new();

    public async Task JoinCall(string orderId, string userName)
    {
        Context.Items["orderId"] = orderId;
        Context.Items["userName"] = userName;

        await Groups.AddToGroupAsync(Context.ConnectionId, orderId);

        ActiveUsers.AddOrUpdate(orderId,
            new HashSet<string> { userName },
            (key, existingUsers) =>
            {
                existingUsers.Add(userName);
                return existingUsers;
            });

        await Clients.Group(orderId).SendAsync("UserJoined", userName, ActiveUsers[orderId]);
    }

    public async Task LeaveCall(string orderId, string userName)
    {
        if (!ActiveUsers.TryGetValue(orderId, out var users)) return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, orderId);

        users.Remove(userName);
        if (users.Count == 0)
        {
            ActiveUsers.TryRemove(orderId, out _);
        }

        await Clients.Group(orderId).SendAsync("UserLeft", userName, ActiveUsers.ContainsKey(orderId) ? ActiveUsers[orderId] : new HashSet<string>());
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        string connectionId = Context.ConnectionId;
        string orderId = Context.Items.ContainsKey("orderId") ? Context.Items["orderId"].ToString() : null;
        string userName = Context.Items.ContainsKey("userName") ? Context.Items["userName"].ToString() : null;

        if (!string.IsNullOrEmpty(orderId) && !string.IsNullOrEmpty(userName))
        {
            await LeaveCall(orderId, userName);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task StartCall(string orderId, string offer)
    {
        await Clients.Group(orderId).SendAsync("ReceiveCall", new { offer });
    }

    public async Task SendICECandidate(string orderId, string candidate)
    {
        await Clients.Group(orderId).SendAsync("ReceiveICECandidate", candidate);
    }

    public async Task SendSignal(string orderId, string user, string signal)
    {
        await Clients.Group(orderId).SendAsync("ReceiveSignal", user, signal);
    }

    public async Task SendMessage(string orderId, string userId, string message)
    {
        await Clients.Group(orderId).SendAsync("ReceiveMessage", userId, message);
    }

    public async Task ToggleMute(string orderId, string userName, bool isMuted)
    {
        await Clients.Group(orderId).SendAsync("UserMuted", userName, isMuted);
    }

    public async Task ToggleVideo(string orderId, string userName, bool isVideoOff)
    {
        await Clients.Group(orderId).SendAsync("UserVideoToggled", userName, isVideoOff);
    }
}
