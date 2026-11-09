using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using RaceDayAPI.Services;
using System.Security.Claims;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IBlobService _blobs;

    public EventsController(RaceDayDbContext db, IBlobService blobs)
    {
        _db = db;
        _blobs = blobs;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue("sub") ?? "0");

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Event>>> GetAll() =>
        await _db.Events.Include(e => e.Organiser).Include(e => e.Categories).ToListAsync();

    // ---- GET /api/events/my — events organised by the logged-in organiser ----
    [HttpGet("my")]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<IEnumerable<Event>>> GetMine() =>
        await _db.Events
            .Include(e => e.Organiser)
            .Include(e => e.Categories)
            .Where(e => e.OrganiserId == CurrentUserId)
            .ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Event>> GetById(int id)
    {
        var ev = await _db.Events
            .Include(e => e.Organiser)
            .Include(e => e.Categories)
            .FirstOrDefaultAsync(e => e.EventId == id);

        return ev == null ? NotFound() : ev;
    }

    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<Event>> Create(Event ev)
    {
        if (!new[] { "Run", "Walk", "Cycle" }.Contains(ev.EventType))
            return BadRequest("EventType must be Run, Walk, or Cycle.");

        if (ev.Distance <= 0)
            return BadRequest("Distance must be greater than 0.");

        ev.OrganiserId = CurrentUserId; // organiser is always the logged-in user

        if (!await _db.Users.AnyAsync(u => u.UserId == ev.OrganiserId))
            return BadRequest("Organiser does not exist.");

        ev.Organiser = null;      // avoid re-inserting nav property
        ev.Categories.Clear();

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, ev);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int id, Event updated)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();

        if (ev.OrganiserId != CurrentUserId)
            return Forbid();

        if (!new[] { "Run", "Walk", "Cycle" }.Contains(updated.EventType))
            return BadRequest("EventType must be Run, Walk, or Cycle.");

        if (updated.Distance <= 0)
            return BadRequest("Distance must be greater than 0.");

        ev.Name = updated.Name;
        ev.Description = updated.Description;
        ev.Date = updated.Date;
        ev.Location = updated.Location;
        ev.Distance = updated.Distance;
        ev.EventType = updated.EventType;
        ev.BannerImageUrl = updated.BannerImageUrl;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ---- POST /api/events/{id}/banner — Organiser uploads a banner for own event ----
    [HttpPost("{id:int}/banner")]
    [Authorize(Roles = "Organiser")]
    [RequestSizeLimit(10_000_000)] // 10 MB
    public async Task<IActionResult> UploadBanner(int id, IFormFile file)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound(new { message = "Event not found." });

        if (ev.OrganiserId != CurrentUserId)
            return Forbid();

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        var fileName = $"event-{id}-{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        ev.BannerImageUrl = await _blobs.UploadAsync(file, fileName);
        await _db.SaveChangesAsync();

        return Ok(new { bannerImageUrl = ev.BannerImageUrl });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();

        if (ev.OrganiserId != CurrentUserId)
            return Forbid();

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
