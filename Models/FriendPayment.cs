using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VideoDiningApp.Models
{
    public class FriendPayment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; }

        [Required] 
        public int FriendId { get; set; }

        [Required]
        public string FriendEmail { get; set; }

        [ForeignKey(nameof(FriendId))]
        public User Friend { get; set; }

        public bool HasPaid { get; set; }
    }
}
