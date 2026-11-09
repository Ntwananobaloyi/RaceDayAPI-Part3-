using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using System.Security.Claims;

namespace RaceDayAPI.Controllers;
[ApiController]
[Route("api/[controller]")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public EnrolmentsController(RaceDayDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirstValue("sub") ?? "0");

    [HttpGet]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<IEnumerable<Enrolment>>> GetAll() =>
        await _db.Enrolments
            .Include(e => e.Participant)
            .Include(e => e.Event)
            .Include(e => e.Category)
            .Include(e => e.Result)
            .ToListAsync();

    // ---- GET /api/enrolments/my — the logged-in participant's enrolments ----
    [HttpGet("my")]
    [Authorize(Roles = "Participant")]
    public async Task<ActionResult<IEnumerable<Enrolment>>> GetMine() =>
        await _db.Enrolments
            .Include(e => e.Event)
            .Include(e => e.Category)
            .Include(e => e.Result)
            .Where(e => e.ParticipantId == CurrentUserId)
            .ToListAsync();

    // ---- GET /api/enrolments/by-event/{eventId} — organiser sees enrolments for own event ----
    [HttpGet("by-event/{eventId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<IEnumerable<Enrolment>>> GetByEvent(int eventId)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null) return NotFound();

        if (ev.OrganiserId != CurrentUserId)
            return Forbid();

        return await _db.Enrolments
            .Include(e => e.Participant)
            .Include(e => e.Category)
            .Include(e => e.Result)
            .Where(e => e.EventId == eventId)
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Enrolment>> GetById(int id)
    {
        var enr = await _db.Enrolments
            .Include(e => e.Participant)
            .Include(e => e.Event)
            .Include(e => e.Category)
            .Include(e => e.Result)
            .FirstOrDefaultAsync(e => e.EnrolmentId == id);

        if (enr == null) return NotFound();

        if (enr.ParticipantId != CurrentUserId && enr.Event?.OrganiserId != CurrentUserId)
            return Forbid();

        return enr;
    }

    [HttpGet("by-participant/{participantId:int}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Enrolment>>> GetByParticipant(int participantId)
    {
        if (participantId != CurrentUserId && !User.IsInRole("Organiser"))
            return Forbid();

        return await _db.Enrolments
            .Include(e => e.Event)
            .Include(e => e.Category)
            .Where(e => e.ParticipantId == participantId)
            .ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = "Participant")]
    public async Task<ActionResult<Enrolment>> Create(Enrolment enrolment)
    {
        enrolment.ParticipantId = CurrentUserId; // server-forced, never trusted from the client

        if (await _db.Enrolments.AnyAsync(e =>
                e.ParticipantId == enrolment.ParticipantId && e.EventId == enrolment.EventId))
            return Conflict("Participant is already enrolled in this event.");

        if (!await _db.Users.AnyAsync(u => u.UserId == enrolment.ParticipantId))
            return BadRequest("Participant does not exist.");

        var category = await _db.Categories.FindAsync(enrolment.CategoryId);
        if (category == null) return BadRequest("Category does not exist.");
        if (category.EventId != enrolment.EventId)
            return BadRequest("Category does not belong to that event.");

        enrolment.Participant = null;
        enrolment.Event = null;
        enrolment.Category = null;
        enrolment.Result = null;
        enrolment.Status = "Pending";
        enrolment.EnrolmentDate = DateTime.UtcNow;

        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = enrolment.EnrolmentId }, enrolment);
    }

    [HttpPatch("{id:int}/confirm")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Confirm(int id)
    {
        var enr = await _db.Enrolments.Include(e => e.Event)
            .FirstOrDefaultAsync(e => e.EnrolmentId == id);
        if (enr == null) return NotFound();

        if (enr.Event?.OrganiserId != CurrentUserId)
            return Forbid();

        enr.Status = "Confirmed";
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var enr = await _db.Enrolments.Include(e => e.Event)
            .FirstOrDefaultAsync(e => e.EnrolmentId == id);
        if (enr == null) return NotFound();

        // the participant can withdraw; the event's organiser can remove
        if (enr.ParticipantId != CurrentUserId && enr.Event?.OrganiserId != CurrentUserId)
            return Forbid();

        _db.Enrolments.Remove(enr);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
