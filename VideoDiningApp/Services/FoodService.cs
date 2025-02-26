using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VideoDiningApp.Data;
using VideoDiningApp.Models;

public class FoodService : IFoodService
{
    private readonly AppDbContext _context;

    public FoodService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<FoodItem>> GetFoodItemsByIdsAsync(List<int> foodItemIds)
    {
        return await _context.FoodItems.Where(f => foodItemIds.Contains(f.Id)).ToListAsync();
    }
}
