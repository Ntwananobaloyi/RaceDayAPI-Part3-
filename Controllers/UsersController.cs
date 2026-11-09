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
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public UsersController(RaceDayDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirstValue("sub") ?? "0");

    // ---- GET /api/users/me — the logged-in user's own profile ----
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var user = await _db.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
        if (user == null) return NotFound();

        return Ok(new
        {
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            RoleName = user.Role?.RoleName ?? "",
            user.ProfilePictureUrl,
            user.CreatedAt
        });
    }

    // ---- PUT /api/users/me — update own profile (optionally change password) ----
    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateMe(UpdateProfileRequest req)
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(req.Email) && req.Email != user.Email)
        {
            if (await _db.Users.AnyAsync(u => u.Email == req.Email && u.UserId != user.UserId))
                return Conflict("Email already in use.");
            user.Email = req.Email;
        }

        if (!string.IsNullOrWhiteSpace(req.FirstName)) user.FirstName = req.FirstName;
        if (!string.IsNullOrWhiteSpace(req.LastName)) user.LastName = req.LastName;
        user.ProfilePictureUrl = req.ProfilePictureUrl;

        if (!string.IsNullOrWhiteSpace(req.NewPassword))
        {
            if (!PasswordHasher.Verify(req.CurrentPassword ?? "", user.PasswordHash))
                return BadRequest("Current password is incorrect.");
            if (req.NewPassword.Length < 8)
                return BadRequest("New password must be at least 8 characters.");

            user.PasswordHash = PasswordHasher.Hash(req.NewPassword);
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<IEnumerable<User>>> GetAll() =>
        await _db.Users.Include(u => u.Role).ToListAsync();

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<User>> GetById(int id)
    {
        var user = await _db.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == id);
        return user == null ? NotFound() : user;
    }

    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<User>> Create(CreateUserRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return Conflict("Email already in use.");

        if (!await _db.Roles.AnyAsync(r => r.RoleId == req.RoleId))
            return BadRequest("Role does not exist.");

        var user = new User
        {
            Email = req.Email,
            FirstName = req.FirstName,
            LastName = req.LastName,
            RoleId = req.RoleId,
            ProfilePictureUrl = req.ProfilePictureUrl,
            // PasswordHash is [JsonIgnore], so the plain password arrives via the request model and is hashed here
            PasswordHash = PasswordHasher.Hash(req.Password),
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = user.UserId }, user);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int id, User updated)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.FirstName = updated.FirstName;
        user.LastName = updated.LastName;
        user.Email = updated.Email;
        user.RoleId = updated.RoleId;
        user.ProfilePictureUrl = updated.ProfilePictureUrl;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
