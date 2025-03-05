using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoDiningApp.Data;
using VideoDiningApp.Hubs;
using VideoDiningApp.Models;
using VideoDiningApp.Services;

[Route("api/video-calls")]
[ApiController]
public class VideoCallController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHubContext<VideoCallHub> _videoCallHub;
    private readonly IVideoCallService _videoCallService;
    private readonly ILogger<VideoCallController> _logger;

    public VideoCallController(AppDbContext context, IHubContext<VideoCallHub> videoCallHub, IVideoCallService videoCallService, ILogger<VideoCallController> logger)
    {
        _context = context;
        _videoCallHub = videoCallHub;
        _videoCallService = videoCallService; 
        _logger = logger;
    }

    [HttpPost("request")]
    public async Task<IActionResult> SendCallRequest([FromBody] VideoCallRequestDto request)
    {
        int authenticatedUserId = GetAuthenticatedUserId();
        if (request == null || request.FriendIds == null || request.FriendIds.Count == 0)
            return BadRequest(new { message = "Invalid call request." });

        if (request.UserId != authenticatedUserId)
            return Unauthorized(new { message = "Unauthorized call request." });

        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null)
            return NotFound(new { message = "User not found." });

        List<int> failedRequests = new List<int>();

        foreach (var friendId in request.FriendIds)
        {
            var friend = await _context.Users.FindAsync(friendId);
            if (friend == null)
            {
                failedRequests.Add(friendId);
                continue;
            }

            var existingRequest = await _context.VideoCallRequests
                .FirstOrDefaultAsync(r => r.UserId == request.UserId && r.FriendId == friendId && r.Status == "Pending");

            if (existingRequest != null)
            {
                failedRequests.Add(friendId);
                continue;
            }

            var newRequest = new VideoCallRequest
            {
                UserId = request.UserId,
                FriendId = friendId,
                RoomUrl = $"https://meet.jit.si/{Guid.NewGuid()}",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.VideoCallRequests.Add(newRequest);
            await _context.SaveChangesAsync();

            await _videoCallHub.Clients.User(friendId.ToString()).SendAsync("ReceiveCallNotification", "You have a new video call request.");
        }

        if (failedRequests.Count > 0)
            return BadRequest(new { message = "Some requests failed.", failedFriendIds = failedRequests });

        return Ok(new { message = "Video call request sent successfully." });
    }

    [HttpPost("accept/{id}")]
    public async Task<IActionResult> AcceptCallRequest(int id)
    {
        var callRequest = await _context.VideoCallRequests.FindAsync(id);
        if (callRequest == null || callRequest.Status != "Pending")
            return NotFound(new { message = "Call request not found or already processed." });

        callRequest.Status = "Accepted";
        callRequest.StartedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _videoCallHub.Clients.User(callRequest.UserId.ToString()).SendAsync("StartVideoCall", callRequest.RoomUrl);

        return Ok(new { message = "Call request accepted.", videoCallUrl = callRequest.RoomUrl });
    }

    [HttpPost("reject")]
    public async Task<IActionResult> RejectCall([FromBody] RejectCallRequest request)
    {
        var result = await _videoCallService.RejectCallAsync(request.UserId, request.CallId);
        if (!result)
            return BadRequest("Failed to reject call.");

        return Ok("Call rejected successfully.");
    }

    // ✅ 3️⃣ Call End API
    [HttpPost("end")]
    public async Task<IActionResult> EndCall([FromBody] EndCallRequest request)
    {
        var result = await _videoCallService.EndCallAsync(request.CallId);
        if (!result)
            return BadRequest("Failed to end call.");

        return Ok("Call ended successfully.");
    }

    [HttpGet("active-calls")]
    public async Task<IActionResult> GetActiveCalls()
    {
        var activeCalls = await _context.VideoCallRequests
            .Where(r => r.Status == "Accepted")
            .Select(r => new { r.Id, r.UserId, r.FriendId, r.RoomUrl })
            .ToListAsync();

        return Ok(activeCalls);
    }

    [HttpGet("status/{id}")]
    public async Task<IActionResult> GetCallRequestStatus(int id)
    {
        var callRequest = await _context.VideoCallRequests.FindAsync(id);
        if (callRequest == null)
            return NotFound(new { message = "Call request not found." });

        return Ok(new { callRequest.Id, callRequest.Status, callRequest.RoomUrl });
    }

    private int GetAuthenticatedUserId()
    {
        return 1; 
    }
}

public class VideoCallRequestDto
{
    public int UserId { get; set; }
    public List<int> FriendIds { get; set; }
}

public class RejectCallRequest
{
    public int UserId { get; set; }
    public Guid CallId { get; set; }
}

public class EndCallRequest
{
    public Guid CallId { get; set; }
}