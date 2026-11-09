using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using System.Security.Claims;

namespace RaceDayAPI.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public ResultsController(RaceDayDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirstValue("sub") ?? "0");

    [HttpGet]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<IEnumerable<Result>>> GetAll() =>
        await _db.Results.Include(r => r.Enrolment).ToListAsync();

    // ---- GET /api/results/my — the logged-in participant's results ----
    [HttpGet("my")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMine()
    {
        var mine = await _db.Results
            .Where(r => r.Enrolment!.ParticipantId == CurrentUserId)
            .Select(r => new
            {
                r.ResultId,
                r.FinishTime,
                r.FinishPosition,
                EventId = r.Enrolment!.EventId,
                EventName = r.Enrolment.Event!.Name,
                CategoryName = r.Enrolment.Category!.Name
            })
            .ToListAsync();

        return Ok(mine);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Result>> GetById(int id)
    {
        var result = await _db.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.Event)
            .FirstOrDefaultAsync(r => r.ResultId == id);
        if (result == null) return NotFound();

        if (result.Enrolment?.ParticipantId != CurrentUserId &&
            result.Enrolment?.Event?.OrganiserId != CurrentUserId)
            return Forbid();

        return result;
    }

    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<Result>> Create(Result result)
    {
        if (result.FinishPosition <= 0)
            return BadRequest("FinishPosition must be greater than 0.");

        var enrolment = await _db.Enrolments.Include(e => e.Event)
            .FirstOrDefaultAsync(e => e.EnrolmentId == result.EnrolmentId);
        if (enrolment == null)
            return BadRequest("Enrolment does not exist.");

        if (enrolment.Event?.OrganiserId != CurrentUserId)
            return Forbid();

        if (await _db.Results.AnyAsync(r => r.EnrolmentId == result.EnrolmentId))
            return Conflict("A result already exists for this enrolment.");

        result.Enrolment = null;

        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = result.ResultId }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int id, Result updated)
    {
        var result = await _db.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.Event)
            .FirstOrDefaultAsync(r => r.ResultId == id);
        if (result == null) return NotFound();

        if (result.Enrolment?.Event?.OrganiserId != CurrentUserId)
            return Forbid();

        if (updated.FinishPosition <= 0)
            return BadRequest("FinishPosition must be greater than 0.");

        result.FinishTime = updated.FinishTime;
        result.FinishPosition = updated.FinishPosition;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _db.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.Event)
            .FirstOrDefaultAsync(r => r.ResultId == id);
        if (result == null) return NotFound();

        if (result.Enrolment?.Event?.OrganiserId != CurrentUserId)
            return Forbid();

        _db.Results.Remove(result);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
