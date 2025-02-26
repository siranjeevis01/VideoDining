using Microsoft.EntityFrameworkCore;
using VideoDiningApp.Models;
using VideoDiningApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace VideoDiningApp.Repositories
{
    public class FriendshipRepository : IFriendshipRepository
    {
        private readonly AppDbContext _context;
        public FriendshipRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Friendship> GetFriendshipAsync(int user1Id, int user2Id)
        {
            return await _context.Friendships  
                .Include(f => f.User1)
                .Include(f => f.User2)
                .FirstOrDefaultAsync(f => (f.User1Id == user1Id && f.User2Id == user2Id) ||
                                          (f.User1Id == user2Id && f.User2Id == user1Id));
        }

        public async Task<IEnumerable<User>> GetFriendsAsync(int userId)
        {
            var friendship = await _context.Friendships 
                .Where(f => f.User1Id == userId || f.User2Id == userId)
                .Include(f => f.User1)
                .Include(f => f.User2)
                .ToListAsync();

            return friendship.Select(f => f.User1Id == userId ? f.User2 : f.User1);
        }

        public async Task AddFriendshipAsync(Friendship friendship)
        {
            _context.Friendships.Add(friendship); 
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFriendshipAsync(Friendship friendship)
        {
            _context.Friendships.Remove(friendship); 
            await _context.SaveChangesAsync();
        }

        public async Task BlockUserAsync(int user1Id, int user2Id)
        {
            Console.WriteLine($"DEBUG: BlockUserAsync called with user1Id={user1Id}, user2Id={user2Id}");

            var friendship = await _context.Friendships 
                .FirstOrDefaultAsync(f => (f.User1Id == user1Id && f.User2Id == user2Id) ||
                                          (f.User1Id == user2Id && f.User2Id == user1Id));

            if (friendship == null)
            {
                Console.WriteLine("DEBUG: Friendship not found. Cannot block.");
                throw new InvalidOperationException("Friendship not found");
            }

            friendship.IsBlocked = true;
            friendship.BlockedByUserId = user1Id; 

            await _context.SaveChangesAsync();

            Console.WriteLine("DEBUG: User blocked successfully.");
        }

        public async Task UpdateFriendshipAsync(Friendship friendship)
        {
            _context.Friendships.Update(friendship);
            await _context.SaveChangesAsync();
        }

        public async Task<object> GetfriendshiptatusAsync(int user1Id, int user2Id)
        {
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => (f.User1Id == user1Id && f.User2Id == user2Id) ||
                                          (f.User1Id == user2Id && f.User2Id == user1Id));

            if (friendship == null)
            {
                return new { status = "Not Friends", message = "No friendship exists between these users." };
            }

            if (friendship.IsBlocked)
            {
                return new { status = "Blocked", message = $"User {friendship.BlockedByUserId} has blocked the other user." };
            }

            return new { status = "Friends", message = "These users are friends." };
        }
    }
}
