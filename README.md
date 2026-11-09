RaceDay

RaceDay is a race-event platform for South African sports events. Organisers create events (runs, walks and cycles), add categories, confirm enrolments and capture results. Participants browse events, enrol in a category, track their enrolments and view their race history.

The solution has two projects:

Project	What it is
RaceDayAPI	RESTful ASP.NET Core Web API (.NET 8), in-memory EF Core database, JWT authentication, BCrypt password hashing, Swagger
RaceDayMVC	ASP.NET Core MVC web front end (.NET 8). It talks to the API only, stores the JWT in session, and can be run in Docker
