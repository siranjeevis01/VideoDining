using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VideoDiningApp.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }

        public ICollection<Friend> Friends { get; set; } = new List<Friend>();

        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public ICollection<VideoCall> VideoCalls { get; set; } = new List<VideoCall>();

        public ICollection<FriendPayment> FriendPayments { get; set; }
    }
}
