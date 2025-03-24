using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using VideoDiningApp.Hubs;
using VideoDiningApp.Models;
using VideoDiningApp.Data;  

namespace VideoDiningApp.Services
{
    public class VideoCallService : IVideoCallService
    {
        private static ConcurrentDictionary<int, bool> ActiveCalls = new();
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ApplicationDbContext _context;

        public VideoCallService(IHubContext<NotificationHub> hubContext, ApplicationDbContext context)
        {
            _hubContext = hubContext;
            _context = context;
        }

        public async Task StartCallForOrderAsync(int orderId)
        {
            if (!ActiveCalls.ContainsKey(orderId))
            {
                ActiveCalls[orderId] = true;
                Console.WriteLine($"✅ Video call started for Order ID: {orderId}");

                await _hubContext.Clients.Group(orderId.ToString()) 
                    .SendAsync("StartVideoCall", "🍽️ Your food has arrived! Join the video call.");
            }
        }

        public async Task EndCallForOrderAsync(int orderId)
        {
            if (ActiveCalls.ContainsKey(orderId))
            {
                ActiveCalls.TryRemove(orderId, out _);
                Console.WriteLine($"🚪 Video call ended for Order ID: {orderId}");

                await _hubContext.Clients.Group(orderId.ToString()) 
                    .SendAsync("EndVideoCall", "🚪 The video call has ended.");
            }
        }

        public async Task LogCallHistory(int orderId, DateTime endTime)
        {
            var callHistory = new VideoCall
            {
                OrderId = orderId,
                CallEndTime = endTime,
                CallStatus = "Ended",
            };

            _context.VideoCalls.Add(callHistory);
            await _context.SaveChangesAsync();
        }

        public List<VideoCall> GetCallHistory(int userId)
        {
            return _context.VideoCalls
                .Where(call => call.UserId == userId || call.FriendUserId == userId)
                .ToList();
        }
    }
}
