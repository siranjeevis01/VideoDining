using System;
using System.Linq;
using System.Threading.Tasks;
using VideoDiningApp.Data;
using VideoDiningApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace VideoDiningApp.Services
{
    public class VideoCallService : IVideoCallService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VideoCallService> _logger;

        public VideoCallService(AppDbContext context, ILogger<VideoCallService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ✅ Call Rejection Logic
        public async Task<bool> RejectCallAsync(int userId, Guid callId)
        {
            var call = await _context.VideoCalls
                .Include(vc => vc.Participants)
                .FirstOrDefaultAsync(vc => vc.Id == callId);

            if (call == null)
            {
                _logger.LogWarning($"Call {callId} not found.");
                return false;
            }

            var participant = call.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
            {
                _logger.LogWarning($"User {userId} is not part of call {callId}.");
                return false;
            }

            // Mark user as rejected
            participant.HasRejected = true;

            // If all participants reject, cancel the call
            if (call.Participants.All(p => p.HasRejected))
            {
                call.Status = "Canceled";
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ Handle User Disconnects
        public async Task HandleUserDisconnectedAsync(int userId)
        {
            var activeCall = await _context.VideoCalls
                .Include(vc => vc.Participants)
                .FirstOrDefaultAsync(vc => vc.Participants.Any(p => p.UserId == userId));

            if (activeCall == null)
            {
                _logger.LogWarning($"No active call found for user {userId}.");
                return;
            }

            var participant = activeCall.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant != null)
            {
                participant.IsConnected = false;
                _logger.LogInformation($"User {userId} marked as disconnected in call {activeCall.Id}");
            }

            // If all users disconnected, end the call
            if (activeCall.Participants.All(p => !p.IsConnected))
            {
                activeCall.Status = "Ended";
                _logger.LogInformation($"Call {activeCall.Id} ended because all users disconnected.");
            }

            await _context.SaveChangesAsync();
        }

        // ✅ End Call Logic
        public async Task<bool> EndCallAsync(Guid callId)
        {
            var call = await _context.VideoCalls
                .Include(vc => vc.Participants)
                .FirstOrDefaultAsync(vc => vc.Id == callId);

            if (call == null)
            {
                _logger.LogWarning($"Call {callId} not found.");
                return false;
            }

            call.Status = "Ended";
            foreach (var participant in call.Participants)
            {
                participant.IsConnected = false;
            }

            _logger.LogInformation($"Call {callId} ended successfully.");
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
