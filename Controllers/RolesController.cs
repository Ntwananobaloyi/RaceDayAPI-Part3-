using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public RolesController(RaceDayDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Role>>> GetAll() =>
        await _db.Roles.ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Role>> GetById(int id)
    {
        var role = await _db.Roles.FindAsync(id);
        return role == null ? NotFound() : role;
    }
}