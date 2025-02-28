using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VideoDiningApp.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        public int CartId { get; set; }
        public Guid GroupOrderId { get; set; }
        public int FoodItemId { get; set; }
        public int Quantity { get; set; }

        [ForeignKey("CartId")]
        public Cart Cart { get; set; }

        [ForeignKey("FoodItemId")]
        public FoodItem FoodItem { get; set; }
    }
}
