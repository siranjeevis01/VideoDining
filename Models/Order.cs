using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Linq;

namespace VideoDiningApp.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [JsonIgnore]
        public User? User { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public required string Status { get; set; } = "Pending";
        public string? PaymentOtp { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpectedDeliveryTime { get; set; }

        public ICollection<OrderFriend> OrderFriends { get; set; } = new List<OrderFriend>();

        public List<OrderPayment> OrderPayments { get; set; } = new List<OrderPayment>();

        public ICollection<FriendPayment> FriendPayments { get; set; } = new List<FriendPayment>();
        public List<User> Users { get; set; } = new List<User>();

        public List<int> PaidUsers => OrderPayments?.Where(p => p.HasPaid)?.Select(p => p.UserId)?.ToList() ?? new List<int>();
        public List<int> UnpaidUsers => OrderPayments?.Where(p => !p.HasPaid)?.Select(p => p.UserId)?.ToList() ?? new List<int>();

        public string PaymentStatus { get; set; } = "Pending";

        [JsonIgnore]
        public bool AllFriendsPaid => FriendPayments != null && FriendPayments.All(fp => fp.HasPaid);

        [JsonIgnore]
        public bool IsPaymentSuccessful => PaymentStatus == "Paid" && OrderPayments != null && OrderPayments.All(p => p.HasPaid);

        public Order()
        {
            OrderPayments = GetOrderPayments().ToList();
        }

        public IEnumerable<OrderPayment> GetOrderPayments()
        {
            return new List<OrderPayment>();
        }

        public void UpdatePaymentStatus()   
        {
            if (OrderPayments.All(p => p.HasPaid))
            {
                PaymentStatus = "Paid"; 
            }
            else
            {
                PaymentStatus = "Pending";
            }
        }
    }
}
