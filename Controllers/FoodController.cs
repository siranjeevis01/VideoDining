using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoDiningApp.Data;
using VideoDiningApp.Models;

namespace VideoDiningApp.Controllers
{
    [Route("api/foods")]
    [ApiController]
    public class FoodController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FoodController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetFoods()
        {
            try
            {
                var foods = await _context.FoodItems.ToListAsync();
                if (foods.Count == 0)
                {
                    return NotFound(new { message = "No food items found." });
                }
                return Ok(foods); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching foods: {ex.Message}");
                return BadRequest(new { message = "Error fetching foods", details = ex.Message });
            }
        }
    }
}
