using Backend.Models;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController: ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _db.Categories.ToListAsync();
        return Ok(categories);
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<IActionResult> AddCategory([FromBody] Category category)
    {
        _db.Add(category);
        await _db.SaveChangesAsync();
        return Created($"/api/categories/{category.Id}", category);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoriesById(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if(category == null) return NotFound();
        return Ok(category);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var c = await _db.Categories.FindAsync(id);
        if(c == null) return NotFound();
        _db.Categories.Remove(c);
        await _db.SaveChangesAsync();
        return Ok();
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCategory(int id, [FromBody] Category category)
    {
        var c = await _db.Categories.FindAsync(id);
        if (c == null) return NotFound();

        _db.Entry(c).CurrentValues.SetValues(category);
        await _db.SaveChangesAsync();
        return Ok(c);
    }
}
