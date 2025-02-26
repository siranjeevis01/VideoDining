using VideoDiningApp.Models;

namespace VideoDiningApp.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
