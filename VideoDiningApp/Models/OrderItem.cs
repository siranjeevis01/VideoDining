using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using VideoDiningApp.Models;

public class OrderItem
{
    [Key]
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int FoodItemId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public Guid GroupOrderId { get; set; }

    public string UserEmail { get; set; } 

    [ForeignKey("OrderId")]
    public Order Order { get; set; }

    [ForeignKey("FoodItemId")]
    public FoodItem FoodItem { get; set; }
}
