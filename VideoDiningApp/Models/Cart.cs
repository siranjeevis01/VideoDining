using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VideoDiningApp.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public Guid GroupOrderId { get; set; }
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
