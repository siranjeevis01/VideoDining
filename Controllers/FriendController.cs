using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoDiningApp.Data;
using VideoDiningApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace VideoDiningApp.Controllers
{
    [Route("api/friends")]
    [ApiController]
    public class FriendController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FriendController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddFriendRequest([FromBody] FriendRequestEmailDto request)
        {
            if (string.IsNullOrEmpty(request.UserEmail) || string.IsNullOrEmpty(request.FriendEmail))
                return BadRequest(new { message = "Both UserEmail and FriendEmail are required." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.UserEmail);
            var friend = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.FriendEmail);

            if (user == null || friend == null)
                return NotFound(new { message = "User or friend not found." });

            var existingRequest = await _context.FriendRequests
                .AnyAsync(fr => fr.UserId == user.Id && fr.FriendId == friend.Id && fr.Status == "pending");

            if (existingRequest)
                return BadRequest(new { message = "A friend request is already pending." });

            var existingFriendship = await _context.Friends
                .AnyAsync(f => (f.UserId == user.Id && f.FriendId == friend.Id) || (f.UserId == friend.Id && f.FriendId == user.Id));

            if (existingFriendship)
                return BadRequest(new { message = "You are already friends." });

            _context.FriendRequests.Add(new FriendRequest { UserId = user.Id, FriendId = friend.Id, Status = "pending" });
            await _context.SaveChangesAsync();

            return Ok(new { message = "Friend request sent." });
        }

        [HttpPost("accept")]
        public async Task<IActionResult> AcceptFriendRequest([FromBody] AcceptFriendRequest request)
        {
            var friendRequest = await _context.FriendRequests.FindAsync(request.RequestId);
            if (friendRequest == null)
                return NotFound(new { message = "Friend request not found." });

            if (friendRequest.Status != "pending")
                return BadRequest(new { message = "Friend request is not pending." });

            friendRequest.Status = "accepted";

            _context.Friends.Add(new Friend { UserId = friendRequest.UserId, FriendId = friendRequest.FriendId, IsAccepted = true });
            await _context.SaveChangesAsync();

            return Ok(new { message = "Friend request accepted." });
        }

        [HttpPost("reject")]
        public async Task<IActionResult> RejectFriendRequest([FromBody] RejectFriendRequest request)
        {
            var friendRequest = await _context.FriendRequests.FindAsync(request.RequestId);
            if (friendRequest == null)
                return NotFound(new { message = "Friend request not found." });

            _context.FriendRequests.Remove(friendRequest);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Friend request rejected." });
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveFriend([FromQuery] int userId, [FromQuery] int friendId)
        {
            var friendships = await _context.Friends
                .Where(f => (f.UserId == userId && f.FriendId == friendId) ||
                            (f.UserId == friendId && f.FriendId == userId))
                .ToListAsync();

            if (friendships.Any())
            {
                _context.Friends.RemoveRange(friendships);
            }

            var friendRequests = await _context.FriendRequests
                .Where(fr => (fr.UserId == userId && fr.FriendId == friendId) ||
                             (fr.UserId == friendId && fr.FriendId == userId))
                .ToListAsync();

            if (friendRequests.Any())
            {
                _context.FriendRequests.RemoveRange(friendRequests);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Friend removed successfully." });
        }

        [HttpGet("list/{userId}")]
        public async Task<IActionResult> GetFriends(int userId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                return NotFound(new { message = "User not found." });

            var friends = await _context.Friends
                .Where(f => f.UserId == userId || f.FriendId == userId)
                .Include(f => f.User)
                .Include(f => f.FriendUser)
                .Select(f => new
                {
                    FriendId = f.UserId == userId ? f.FriendId : f.UserId,
                    Name = f.UserId == userId ? f.FriendUser.Name : f.User.Name,
                    Email = f.UserId == userId ? f.FriendUser.Email : f.User.Email
                })
                .ToListAsync();

            return Ok(friends);
        }

        [HttpGet("check/{userId}/{friendId}")]
        public async Task<IActionResult> CheckFriendship(int userId, int friendId)
        {
            var friendship = await _context.Friends
                .FirstOrDefaultAsync(f => (f.UserId == userId && f.FriendId == friendId) ||
                                          (f.UserId == friendId && f.FriendId == userId));

            var pendingRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => (fr.UserId == userId && fr.FriendId == friendId) ||
                                           (fr.UserId == friendId && fr.FriendId == userId) && fr.Status == "pending");

            if (friendship != null)
                return Ok(new { isFriend = true, message = "Already friends." });

            if (pendingRequest != null)
                return Ok(new { isFriend = false, message = "Friend request pending." });

            return Ok(new { isFriend = false, message = "No friend request found." });
        }

        [HttpGet("requests/{userId}")]
        public async Task<IActionResult> GetFriendRequests(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var requests = await _context.FriendRequests
                .Where(r => r.FriendId == userId && r.Status == "pending")
                .Include(r => r.User)
                .Select(r => new { r.Id, r.UserId, r.User.Name, r.User.Email })
                .ToListAsync();

            return Ok(requests);
        }
    }
}


public class FriendRequestEmailDto
{
    public string UserEmail { get; set; }
    public string FriendEmail { get; set; }
}

public class AcceptFriendRequest
{
    public int RequestId { get; set; }
}

public class RejectFriendRequest
{
    public int RequestId { get; set; }
}

public class RemoveFriendRequest
{
    public int UserId { get; set; }
    public int FriendId { get; set; }
}
