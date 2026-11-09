using RaceDayAPI.Models;
using System.Text.Json.Serialization;

namespace RaceDayAPI.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        [JsonIgnore]
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}