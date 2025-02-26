using VideoDiningApp.Models;

namespace VideoDiningApp.Repositories
{
    public interface IFriendshipRepository
    {
        Task<Friendship> GetFriendshipAsync(int user1Id, int user2Id);
        Task<IEnumerable<User>> GetFriendsAsync(int userId);
        Task AddFriendshipAsync(Friendship friendship);
        Task RemoveFriendshipAsync(Friendship friendship);
        Task BlockUserAsync(int user1Id, int user2Id);
        Task UpdateFriendshipAsync(Friendship friendship);
    }
}
