using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VideoDiningApp.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int FoodItemId { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public decimal Price { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("FoodItemId")]
        public FoodItem FoodItem { get; set; }
    }
}
