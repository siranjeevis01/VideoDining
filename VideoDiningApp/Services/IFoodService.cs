using System.Collections.Generic;
using System.Threading.Tasks;
using VideoDiningApp.Models;

public interface IFoodService
{
    Task<List<FoodItem>> GetFoodItemsByIdsAsync(List<int> foodItemIds);
}
