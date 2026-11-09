RaceDay

RaceDay is a race-event platform for South African sports events. Organisers create events (runs, walks and cycles), add categories, confirm enrolments and capture results. Participants browse events, enrol in a category, track their enrolments and view their race history.

The solution has two projects:

Project	What it is
RaceDayAPI	RESTful ASP.NET Core Web API (.NET 8), in-memory EF Core database, JWT authentication, BCrypt password hashing, Swagger
RaceDayMVC	ASP.NET Core MVC web front end (.NET 8). It talks to the API only, stores the JWT in session, and can be run in Docker
Repository layout
RaceDayAPI.Prog/
├── RaceDayAPI/
│   ├── Controllers/      Auth, Events, Categories, Enrolments, Results, Users, Roles
│   ├── Data/             RaceDayDbContext (relationships + seed data)
│   ├── Models/           Entities and request/response models
│   ├── Services/         JwtTokenService, PasswordHasher, IBlobService, LocalBlobService
│   ├── wwwroot/uploads/banners/   Uploaded event banners (git-ignored)
│   └── RaceDayAPI.sln    Solution containing both projects
└── RaceDayMVC/
    ├── Controllers/      Account, Participant, Organiser
    ├── Helpers/          AuthCheck (session role checks)
    ├── Models/           View models matching the API's JSON
    ├── Services/         ApiClient (attaches the JWT to every API call)
    ├── Views/            Account, Participant, Organiser, Shared
    ├── Dockerfile
    └── appsettings.Production.json   API address used inside Docker
Prerequisites 
