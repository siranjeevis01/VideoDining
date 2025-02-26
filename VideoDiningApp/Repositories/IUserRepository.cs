using VideoDiningApp.Models;

namespace VideoDiningApp.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByIdAsync(int UserId);
        Task<User> GetUserByEmailAsync(string email);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task<User> GetUserByResetTokenAsync(string token);
    }
}
