using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using VideoDiningApp.Data;
using VideoDiningApp.Models;

[Route("api/foods")]
[ApiController]
public class FoodController : ControllerBase
{
    private readonly AppDbContext _context;

    public FoodController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllFoods()
    {
        var foods = await _context.FoodItems.ToListAsync();

        if (foods == null || !foods.Any())
        {
            return NotFound(new { message = "No available food items." });
        }

        return Ok(foods);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetFoodById(int id)
    {
        var food = await _context.FoodItems.FindAsync(id);  
        if (food == null) return NotFound(new { message = "Food item not found." });

        return Ok(food);
    }

    [HttpPost]
    public async Task<IActionResult> CreateFood([FromBody] FoodItemDto foodDto)
    {
        if (foodDto == null || string.IsNullOrEmpty(foodDto.Name) || foodDto.Price <= 0)
            return BadRequest(new { message = "Invalid food data." });

        var newFood = new FoodItem
        {
            Name = foodDto.Name,
            Price = foodDto.Price,
            Description = foodDto.Description
        };

        _context.FoodItems.Add(newFood);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFoodById), new { id = newFood.Id }, newFood);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFood(int id, [FromBody] FoodItemDto updatedFood)
    {
        var food = await _context.FoodItems.FindAsync(id);
        if (food == null) return NotFound(new { message = "Food item not found." });

        food.Name = updatedFood.Name;
        food.Description = updatedFood.Description;
        food.Price = updatedFood.Price;

        await _context.SaveChangesAsync();

        return Ok(food);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFood(int id)
    {
        var food = await _context.FoodItems.FindAsync(id);  
        if (food == null) return NotFound(new { message = "Food item not found." });

        _context.FoodItems.Remove(food);  
        await _context.SaveChangesAsync();

        return Ok(new { message = "Food item deleted successfully." });
    }

    public class FoodItemDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
    }

}
