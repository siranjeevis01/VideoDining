using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VideoDiningApp.Models
{
    public class Friend
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int FriendId { get; set; }

        public bool IsAccepted { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("FriendId")]
        public User FriendUser { get; set; }

        public ICollection<OrderFriend> OrderFriends { get; set; } = new List<OrderFriend>();
        public ICollection<FriendPayment> FriendPayments { get; set; } = new List<FriendPayment>();
    }
}
