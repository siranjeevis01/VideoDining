using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using VideoDiningApp.Enums;
using VideoDiningApp.Models;

public class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int UserId { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public virtual ICollection<Payment> Payments { get; set; }

    private string _foodItemsSerialized;

    public string FoodItemsSerialized
    {
        get => JsonConvert.SerializeObject(FoodItems);
        set
        {
            _foodItemsSerialized = value;
            _foodItems = string.IsNullOrEmpty(value) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(value);
        }
    }

    private List<int> _foodItems = new List<int>();

    [NotMapped]
    public List<int> FoodItems
    {
        get => _foodItems ?? new List<int>();
        set
        {
            _foodItems = value;
            _foodItemsSerialized = JsonConvert.SerializeObject(value);
        }
    }

    public int TotalFriends { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime EstimatedDeliveryTime { get; set; }
    public bool IsDelivered { get; set; } = false;
    public int? FriendshipId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }

    [Column("GroupOrderId")]
    public Guid GroupOrderId { get; set; }

    [Column("UserEmail")]
    [Required]
    public string UserEmail { get; set; }

    public string Status { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
