using System.ComponentModel.DataAnnotations.Schema;

namespace VideoDiningApp.Models
{
    public class Friendship
    {
        public int Id { get; set; }

        public int User1Id { get; set; }
        public int User2Id { get; set; }

        [ForeignKey("User1Id")]
        public User User1 { get; set; }

        [ForeignKey("User2Id")]
        public User User2 { get; set; }

        public bool IsBlocked { get; set; }
        public int? BlockedByUserId { get; set; }
    }
}
