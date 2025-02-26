using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VideoDiningApp.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int FoodItemId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public Guid GroupOrderId { get; set; }  // ✅ FIXED: Added GroupOrderId

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        [ForeignKey("FoodItemId")]
        public FoodItem FoodItem { get; set; }

        [NotMapped]
        public string UserEmail => Order?.User?.Email ?? string.Empty;
    }
}
