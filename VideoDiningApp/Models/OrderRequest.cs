using System.Collections.Generic;

namespace VideoDiningApp.Models
{
    public class OrderRequest
    {
        public int UserId { get; set; }
        public List<int> FoodItems { get; set; } = new List<int>(); 
    }
}
