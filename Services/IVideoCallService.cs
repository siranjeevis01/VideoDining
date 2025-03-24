using System.Threading.Tasks;

namespace VideoDiningApp.Services
{
    public interface IVideoCallService
    {
        Task StartCallForOrderAsync(int orderId);
        Task EndCallForOrderAsync(int orderId);
        Task LogCallHistory(int orderId, DateTime endTime);
    }
}
