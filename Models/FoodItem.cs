using System.ComponentModel.DataAnnotations;

namespace VideoDiningApp.Models
{
public class FoodItem
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public required string ImageUrl { get; set; }
    public required string Description { get; set; } 
}
}
