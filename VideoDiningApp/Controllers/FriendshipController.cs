using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using VideoDiningApp.Models;
using VideoDiningApp.Repositories;
using VideoDiningApp.Hubs;

namespace VideoDiningApp.Controllers
{
    [Route("api/friends")]
    [ApiController]
    public class FriendshipController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IFriendshipRepository _friendshipRepository;
        private readonly IHubContext<FriendshipHub> _friendshipHub;

        public FriendshipController(IUserRepository userRepository, IFriendshipRepository friendshipRepository, IHubContext<FriendshipHub> friendshipHub)
        {
            _userRepository = userRepository;
            _friendshipRepository = friendshipRepository;
            _friendshipHub = friendshipHub;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddFriend([FromBody] AddFriendRequest request)
        {
            if (request == null || request.User1Id <= 0 || request.User2Id <= 0)
            {
                return BadRequest(new { message = "Invalid data. User1Id and User2Id are required." });
            }

            if (request.User1Id == request.User2Id)
            {
                return BadRequest(new { message = "You cannot add yourself as a friend." });
            }

            var user1 = await _userRepository.GetUserByIdAsync(request.User1Id);
            var user2 = await _userRepository.GetUserByIdAsync(request.User2Id);

            if (user1 == null || user2 == null)
            {
                return NotFound(new { message = "One or both users do not exist." });
            }

            var existingFriendship = await _friendshipRepository.GetFriendshipAsync(request.User1Id, request.User2Id);
            if (existingFriendship != null)
            {
                return Conflict(new { message = "You are already friends." });
            }

            var newFriendship = new Friendship
            {
                User1Id = request.User1Id,
                User2Id = request.User2Id
            };

            await _friendshipRepository.AddFriendshipAsync(newFriendship);
            return Ok(new { message = "Friend added successfully!" });
        }

        [HttpGet("count/{userId}")]
        public async Task<IActionResult> GetFriendsCount(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            var friends = await _friendshipRepository.GetFriendsAsync(userId);
            int friendCount = friends.Count();

            return Ok(new { friendCount });
        }


        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetFriends(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new { message = "Invalid user ID." });
            }

            var friends = await _friendshipRepository.GetFriendsAsync(userId);
            return Ok(friends);
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveFriend([FromBody] RemoveFriendRequest request)
        {
            if (request == null || request.User1Id <= 0 || request.User2Id <= 0)
            {
                return BadRequest(new { message = "User1Id and User2Id are required." });
            }

            var existingFriendship = await _friendshipRepository.GetFriendshipAsync(request.User1Id, request.User2Id);
            if (existingFriendship == null)
            {
                return NotFound(new { message = "Friendship not found." });
            }

            await _friendshipRepository.RemoveFriendshipAsync(existingFriendship);
            return Ok(new { message = "Friend removed successfully!" });
        }

        [HttpPost("block")]
        public async Task<IActionResult> BlockUser([FromBody] BlockUserRequest request)
        {
            if (request == null || request.UserId <= 0 || request.FriendId <= 0)
            {
                return BadRequest(new { message = "UserId and FriendId are required." });
            }

            if (request.UserId == request.FriendId)
            {
                return BadRequest(new { message = "You cannot block yourself." });
            }

            var friendship = await _friendshipRepository.GetFriendshipAsync(request.UserId, request.FriendId);
            if (friendship == null)
            {
                return NotFound(new { message = "Friendship not found." });
            }

            friendship.IsBlocked = true;
            friendship.BlockedByUserId = request.UserId;

            await _friendshipRepository.UpdateFriendshipAsync(friendship);
            return Ok(new { message = "User blocked successfully!" });
        }

        [HttpPost("unblock")]
        public async Task<IActionResult> UnblockUser([FromBody] UnblockUserRequest request)
        {
            if (request == null || request.UserId <= 0 || request.FriendId <= 0)
            {
                return BadRequest(new { message = "UserId and FriendId are required." });
            }

            var friendship = await _friendshipRepository.GetFriendshipAsync(request.UserId, request.FriendId);
            if (friendship == null)
            {
                return NotFound(new { message = "Friendship not found." });
            }

            if (friendship.BlockedByUserId != request.UserId)
            {
                return BadRequest(new { message = "Only the blocker can unblock this user." });
            }

            friendship.IsBlocked = false;
            friendship.BlockedByUserId = null;

            await _friendshipRepository.UpdateFriendshipAsync(friendship);
            return Ok(new { message = "User unblocked successfully!" });
        }

        [HttpGet("status")]
        public async Task<IActionResult> CheckFriendshipStatus([FromQuery] int user1Id, [FromQuery] int user2Id)
        {
            if (user1Id <= 0 || user2Id <= 0)
            {
                return BadRequest(new { message = "Invalid user IDs." });
            }

            if (user1Id == user2Id)
            {
                return BadRequest(new { message = "A user cannot check friendship status with themselves." });
            }

            var friendship = await _friendshipRepository.GetFriendshipAsync(user1Id, user2Id);

            if (friendship == null)
            {
                return Ok(new { status = "Not Friends", message = "These users are not friends." });
            }

            return Ok(new { status = "Friends", message = "These users are friends." });
        }

        [HttpPost("request")]
        public async Task<IActionResult> SendFriendRequest([FromBody] AddFriendRequest request)
        {
            if (request == null || request.User1Id <= 0 || request.User2Id <= 0)
            {
                return BadRequest(new { message = "Invalid request data." });
            }

            if (request.User1Id == request.User2Id)
            {
                return BadRequest(new { message = "You cannot send a friend request to yourself." });
            }

            var user1 = await _userRepository.GetUserByIdAsync(request.User1Id);
            var user2 = await _userRepository.GetUserByIdAsync(request.User2Id);

            if (user1 == null || user2 == null)
            {
                return NotFound(new { message = "One or both users do not exist." });
            }

            var existingRequest = await _friendshipRepository.GetFriendRequestAsync(request.User1Id, request.User2Id);
            if (existingRequest != null)
            {
                return Conflict(new { message = "Friend request already sent." });
            }

            var friendRequest = new FriendRequest
            {
                SenderId = request.User1Id,
                ReceiverId = request.User2Id
            };

            await _friendshipRepository.AddFriendRequestAsync(friendRequest);
            return Ok(new { message = "Friend request sent successfully!" });
        }

        [HttpGet("pending/{userId}")]
        public async Task<IActionResult> GetPendingRequests(int userId)
        {
            var requests = await _friendshipRepository.GetPendingRequestsAsync(userId);
            return Ok(requests);
        }

        [HttpPost("accept")]
        public async Task<IActionResult> AcceptFriendRequest([FromBody] AddFriendRequest request)
        {
            var friendRequest = await _friendshipRepository.GetFriendRequestAsync(request.User1Id, request.User2Id);
            if (friendRequest == null)
            {
                return NotFound(new { message = "Friend request not found." });
            }

            var newFriendship = new Friendship
            {
                User1Id = request.User1Id,
                User2Id = request.User2Id
            };

            await _friendshipRepository.AddFriendshipAsync(newFriendship);
            await _friendshipRepository.RemoveFriendRequestAsync(friendRequest);

            // Notify the sender about the accepted request
            await _friendshipHub.Clients.User(request.User1Id.ToString())
                .SendAsync("FriendRequestAccepted", request.User2Id);

            return Ok(new { message = "Friend request accepted!" });
        }

        [HttpPost("reject")]
        public async Task<IActionResult> RejectFriendRequest([FromBody] AddFriendRequest request)
        {
            var friendRequest = await _friendshipRepository.GetFriendRequestAsync(request.User1Id, request.User2Id);
            if (friendRequest == null)
            {
                return NotFound(new { message = "Friend request not found." });
            }

            await _friendshipRepository.RemoveFriendRequestAsync(friendRequest);

            // Notify the sender about the rejection
            await _friendshipHub.Clients.User(request.User1Id.ToString())
                .SendAsync("FriendRequestRejected", request.User2Id);

            return Ok(new { message = "Friend request rejected." });
        }
    }
}
