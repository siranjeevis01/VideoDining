using System.Threading.Tasks;
using VideoDiningApp.Models;

namespace VideoDiningApp.Repositories
{
    public interface IPaymentRepository
    {
        Task SavePaymentAsync(Payment payment);
        Task<Payment> GetPaymentByOrderIdAsync(int orderId);
    }
}
