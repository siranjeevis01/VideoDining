using System.ComponentModel.DataAnnotations.Schema;

namespace VideoDiningApp.Models
{
    public class FriendRequest
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }

        [ForeignKey("SenderId")]
        public User Sender { get; set; }

        [ForeignKey("ReceiverId")]
        public User Receiver { get; set; }
    }
}
