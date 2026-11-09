using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;

namespace RaceDayAPI.Data
{ 

    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

        // BCrypt hash of "Password123!" - used for all four seeded users
        private const string SeedPasswordHash = "$2a$11$Lyi49VbQJB9IlHtL4YEqoeKNPEP5a2KWiPi45C1RO7lfQo1Todp0O";

        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Enrolment> Enrolments => Set<Enrolment>();
        public DbSet<Result> Results => Set<Result>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Unique constraints
            modelBuilder.Entity<Role>().HasIndex(r => r.RoleName).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Enrolment>().HasIndex(e => new { e.ParticipantId, e.EventId }).IsUnique();
            modelBuilder.Entity<Result>().HasIndex(r => r.EnrolmentId).IsUnique();

            // Relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany(u => u.OrganisedEvents)
                .HasForeignKey(e => e.OrganiserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Categories)
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Participant)
                .WithMany(u => u.Enrolments)
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Result>()
                .HasOne(r => r.Enrolment)
                .WithOne(e => e.Result)
                .HasForeignKey<Result>(r => r.EnrolmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed data — matches the SQL script
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Organiser" },
                new Role { RoleId = 2, RoleName = "Participant" }
            );

            modelBuilder.Entity<User>().HasData(
                new User { UserId = 1, Email = "organiser_sipho@raceday.co.za", PasswordHash = SeedPasswordHash, FirstName = "Sipho", LastName = "Khumalo", RoleId = 1, CreatedAt = new DateTime(2026, 1, 1) },
                new User { UserId = 2, Email = "organiser_chantal@raceday.co.za", PasswordHash = SeedPasswordHash, FirstName = "Chantal", LastName = "Du Toit", RoleId = 1, CreatedAt = new DateTime(2026, 1, 1) },
                new User { UserId = 3, Email = "participant_jabu@raceday.co.za", PasswordHash = SeedPasswordHash, FirstName = "Jabu", LastName = "Adele", RoleId = 2, ProfilePictureUrl = "https://example.com/jabu.jpg", CreatedAt = new DateTime(2026, 1, 1) },
                new User { UserId = 4, Email = "participant_sarah@raceday.co.za", PasswordHash = SeedPasswordHash, FirstName = "Sarah", LastName = "Smith", RoleId = 2, ProfilePictureUrl = "https://example.com/sarah.jpg", CreatedAt = new DateTime(2026, 1, 1) }
            );

            modelBuilder.Entity<Event>().HasData(
                new Event { EventId = 1, Name = "Soweto Half Marathon", Description = "An exciting running event through the historic streets of Soweto.", Date = new DateTime(2026, 11, 1, 6, 0, 0), Location = "Soweto, Johannesburg", Distance = 21.10m, EventType = "Run", OrganiserId = 1 },
                new Event { EventId = 2, Name = "Cape Town Cycle Classic", Description = "A breathtaking cycle tour along the Cape peninsula.", Date = new DateTime(2026, 10, 12, 7, 0, 0), Location = "Cape Town", Distance = 109.00m, EventType = "Cycle", OrganiserId = 2 },
                new Event { EventId = 3, Name = "Gauteng Community Walk", Description = "A fun walk for the whole family promoting healthy living.", Date = new DateTime(2026, 9, 20, 8, 0, 0), Location = "Pretoria", Distance = 5.00m, EventType = "Walk", OrganiserId = 1 }
            );

            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, EventId = 1, Name = "Under 20 (Junior)" },
                new Category { CategoryId = 2, EventId = 1, Name = "Senior (20-39)" },
                new Category { CategoryId = 3, EventId = 1, Name = "Veterans (40+)" },
                new Category { CategoryId = 4, EventId = 2, Name = "Elite Cyclists" },
                new Category { CategoryId = 5, EventId = 2, Name = "Open Category 109km" },
                new Category { CategoryId = 6, EventId = 2, Name = "Junior Cyclists" },
                new Category { CategoryId = 7, EventId = 3, Name = "Fun Walk 5km" },
                new Category { CategoryId = 8, EventId = 3, Name = "Senior Citizen Walk 5km" }
            );

            modelBuilder.Entity<Enrolment>().HasData(
                new Enrolment { EnrolmentId = 1, ParticipantId = 3, EventId = 1, CategoryId = 2, EnrolmentDate = new DateTime(2026, 8, 15, 10, 0, 0), Status = "Confirmed" },
                new Enrolment { EnrolmentId = 2, ParticipantId = 4, EventId = 2, CategoryId = 5, EnrolmentDate = new DateTime(2026, 8, 16, 11, 30, 0), Status = "Confirmed" },
                new Enrolment { EnrolmentId = 3, ParticipantId = 3, EventId = 3, CategoryId = 7, EnrolmentDate = new DateTime(2026, 8, 17, 9, 15, 0), Status = "Confirmed" }
            );

            modelBuilder.Entity<Result>().HasData(
                new Result { ResultId = 1, EnrolmentId = 1, FinishTime = "01:45:23", FinishPosition = 42 },
                new Result { ResultId = 2, EnrolmentId = 2, FinishTime = "03:12:05", FinishPosition = 128 }
            );
        }
    }
}
