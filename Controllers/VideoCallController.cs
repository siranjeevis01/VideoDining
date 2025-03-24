using VideoDiningApp.Models;
using VideoDiningApp.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using VideoDiningApp.Services;

[Route("api/video-call")]
[ApiController]
public class VideoCallController : ControllerBase
{
    private static ConcurrentDictionary<int, HashSet<string>> ActiveCalls = new();
    private readonly IVideoCallService _videoCallService;
    private readonly ApplicationDbContext _context;

    public VideoCallController(IVideoCallService videoCallService, ApplicationDbContext context)
    {
        _videoCallService = videoCallService;
        _context = context;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartCall([FromBody] StartCallRequest request)
    {
        if (!ActiveCalls.ContainsKey(request.OrderId))
        {
            ActiveCalls[request.OrderId] = new HashSet<string>();
        }

        ActiveCalls[request.OrderId].Add(request.UserName);
        var startTime = DateTime.UtcNow;

        var hubContext = HttpContext.RequestServices.GetService<IHubContext<VideoCallHub>>();
        if (hubContext != null)
        {
            await hubContext.Clients.Group(request.OrderId.ToString()).SendAsync("ReceiveCall", new { offer = request.Offer });
        }
        else
        {
            return BadRequest(new { message = "Hub context unavailable" });
        }

        return Ok(new { message = "✅ Video call started", orderId = request.OrderId, startTime });
    }

    [HttpPost("end")]
    public IActionResult EndCall([FromBody] EndCallRequest request)
    {
        if (ActiveCalls.ContainsKey(request.OrderId))
        {
            ActiveCalls.TryRemove(request.OrderId, out _);
            var endTime = DateTime.UtcNow;
            _videoCallService.LogCallHistory(request.OrderId, endTime);

            return Ok(new { message = "🚪 Video call ended", orderId = request.OrderId, endTime });
        }
        return BadRequest(new { message = "⚠️ Call not found" });
    }

    [HttpGet("{orderId}/participants")]
    public IActionResult GetActiveParticipants(int orderId)
    {
        if (ActiveCalls.ContainsKey(orderId))
        {
            return Ok(new { participants = ActiveCalls[orderId].ToList() });
        }
        return NotFound(new { message = "No active participants" });
    }

    [HttpGet("history/{userId}")]
    public IActionResult GetCallHistory(int userId)
    {
        var history = _context.VideoCalls
            .Where(c => c.UserId == userId || c.FriendUserId == userId)
            .OrderByDescending(c => c.CallStartTime)
            .Select(c => new
            {
                c.OrderId,
                c.CallEndTime
            })
            .ToList();

        if (history == null || !history.Any())
        {
            return NotFound(new { message = "No call history found" });
        }

        return Ok(history);
    }

    [HttpGet("{orderId}")]
    public IActionResult CheckCallStatus(int orderId)
    {
        bool isActive = ActiveCalls.ContainsKey(orderId);
        return Ok(new { isActive });
    }
}

public class StartCallRequest
{
    public int OrderId { get; set; }
    public string Offer { get; set; }
    public string UserName { get; set; }
}

public class EndCallRequest
{
    public int OrderId { get; set; }
}
