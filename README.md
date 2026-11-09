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


Running locally
Open RaceDayAPI/RaceDayAPI.sln in Visual Studio.
Check RaceDayAPI/appsettings.json has:
json
   "ApiSettings": { "PublicBaseUrl": "https://localhost:7173/uploads/banners" }
Start RaceDayAPI with the https launch profile. Swagger opens at https://localhost:7173/swagger.
Start RaceDayMVC (right-click the project, then Debug > Start New Instance). RaceDayMVC/appsettings.json already points to https://localhost:7173/.
Log in with a seeded account. The login page redirects to the Organiser dashboard or the Participant events page depending on the role.

Tip: to run both at once, right-click the solution, choose Configure Startup Projects, then select Multiple startup projects.

Testing the API in Swagger
POST /api/auth/login with an email and password, and copy the token from the response.
Click Authorize and paste the token only (Swagger adds Bearer ).
Try the endpoints below. Organiser endpoints reject Participant tokens, and vice versa.
Running the MVC in Docker

Only the MVC runs in Docker. The API keeps running on your machine and the container reaches it through host.docker.internal.

Switch Docker Desktop to Linux containers.
Set RaceDayAPI/appsettings.json to the HTTP address:
json
   "ApiSettings": { "PublicBaseUrl": "http://localhost:5104/uploads/banners" }
Start the API with the http launch profile and confirm Swagger opens at http://localhost:5104/swagger.
From the RaceDayMVC folder:
   docker build --load -t raceday-mvc .
   docker run -d -p 8080:80 --name raceday-mvc raceday-mvc
Open http://localhost:8080.

Useful commands:

docker ps                      # is it running?
docker logs raceday-mvc        # container output
docker stop raceday-mvc        # stop
docker start raceday-mvc       # start again
docker rm -f raceday-mvc       # remove (then rebuild after code changes)

If port 8080 is busy, use -p 8081:80. When you finish, switch PublicBaseUrl back to the https address.

Why these settings
The container sets ASPNETCORE_ENVIRONMENT=Production, so appsettings.Production.json overrides the API address with http://host.docker.internal:5104/.
The container does not trust the Visual Studio dev certificate, so the API must be on HTTP for the Docker demo.
Banner URLs are loaded by your browser, so PublicBaseUrl must use localhost, not host.docker.internal.
How it works

Authentication. POST /api/auth/login returns a JWT plus the user's ID, name and role in one response. The MVC stores them in session and ApiClient sends the token as a Bearer header on every call. Role checks ([Authorize(Roles = "Organiser")]) and ownership checks happen in the API. The MVC's own guards only improve the user experience.

Banner storage. Banners are uploaded to POST /api/events/{id}/banner and handled through the IBlobService interface. LocalBlobService saves the file in wwwroot/uploads/banners/ and returns a URL built from ApiSettings:PublicBaseUrl. The Azure.Storage.Blobs package is installed so a real Azure BlobService can replace the local one by changing one line in Program.cs. No Azure account is needed.

API endpoints
