using Microsoft.Extensions.Logging;
using System.Data;
using System.Text.Json.Serialization;

namespace RaceDayAPI.Models
{ 
public class User
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;

    [JsonIgnore] // never send password hash to client
    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public int RoleId { get; set; }
    public Role? Role { get; set; }

    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();

    [JsonIgnore]
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
}