using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using System.Security.Claims;

namespace RaceDayAPI.Controllers;
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public CategoriesController(RaceDayDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirstValue("sub") ?? "0");

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetAll() =>
        await _db.Categories.Include(c => c.Event).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Category>> GetById(int id)
    {
        var cat = await _db.Categories.Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryId == id);
        return cat == null ? NotFound() : cat;
    }

    [HttpGet("by-event/{eventId:int}")]
    public async Task<ActionResult<IEnumerable<Category>>> GetByEvent(int eventId) =>
        await _db.Categories.Where(c => c.EventId == eventId).ToListAsync();

    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<Category>> Create(Category category)
    {
        var ev = await _db.Events.FindAsync(category.EventId);
        if (ev == null)
            return BadRequest("Event does not exist.");

        if (ev.OrganiserId != CurrentUserId)
            return Forbid();

        category.Event = null;

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, category);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var cat = await _db.Categories.Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryId == id);
        if (cat == null) return NotFound();

        if (cat.Event?.OrganiserId != CurrentUserId)
            return Forbid();

        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
