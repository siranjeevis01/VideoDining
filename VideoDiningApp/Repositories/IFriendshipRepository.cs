using VideoDiningApp.Models;

namespace VideoDiningApp.Repositories
{
    public interface IFriendshipRepository
    {
        Task<Friendship> GetFriendshipAsync(int user1Id, int user2Id);
        Task<IEnumerable<User>> GetFriendsAsync(int userId);
        Task<FriendRequest> GetFriendRequestAsync(int senderId, int receiverId);
        Task AddFriendshipAsync(Friendship friendship);
        Task AddFriendRequestAsync(FriendRequest request);
        Task RemoveFriendshipAsync(Friendship friendship);
        Task RemoveFriendRequestAsync(FriendRequest request);
        Task<IEnumerable<FriendRequest>> GetPendingRequestsAsync(int userId);
        Task BlockUserAsync(int user1Id, int user2Id);
        Task UpdateFriendshipAsync(Friendship friendship);
    }
}
