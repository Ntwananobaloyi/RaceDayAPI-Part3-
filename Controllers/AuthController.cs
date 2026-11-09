using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using RaceDayAPI.Services;
using System.Text.RegularExpressions;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly JwtTokenService _jwt;
    public AuthController(RaceDayDbContext db, JwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        // check if email is valid or not using regex
        if (!Regex.IsMatch(req.Email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return BadRequest("Invalid email format. Please enter valid email");

        // password rules (same rules as the original register endpoint)
        var password = req.Password ?? "";
        bool hasCapital = Regex.IsMatch(password, @"[A-Z]");
        bool hasSmall = Regex.IsMatch(password, @"[a-z]");
        bool hasNumber = Regex.IsMatch(password, @"[0-9]");
        bool hasSpecial = Regex.IsMatch(password, @"[^a-zA-Z0-9\s]");
        bool hasLength = password.Length >= 8;

        if (!hasCapital || !hasSmall || !hasNumber || !hasSpecial || !hasLength)
            return BadRequest("Password must contain at least 8 characters," +
                " including a capital letter, a small letter, " +
                "a number and a special character");

        if (string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName))
            return BadRequest("First name and last name are required.");

        var role = await _db.Roles.FirstOrDefaultAsync(r =>
            r.RoleName.ToLower() == (req.RoleName ?? "").ToLower());
        if (role == null)
            return BadRequest("RoleName must be Organiser or Participant.");

        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return Conflict("Email already in use.");

        var user = new User
        {
            Email = req.Email!,
            PasswordHash = PasswordHasher.Hash(password),
            FirstName = req.FirstName.Trim(),
            LastName = req.LastName.Trim(),
            RoleId = role.RoleId,
            ProfilePictureUrl = req.ProfilePictureUrl,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { message = "User registered successfully", userId = user.UserId });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var user = await _db.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == req.Email);

        if (user == null || !PasswordHasher.Verify(req.Password ?? "", user.PasswordHash))
            return Unauthorized("Invalid email or password.");

        var (token, expires) = _jwt.CreateToken(user);

        return new AuthResponse
        {
            Token = token,
            ExpiresAt = expires,
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RoleName = user.Role?.RoleName ?? ""
        };
    }
}
