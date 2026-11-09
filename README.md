# RaceDay | PROG6212 Part 3

## My project

For Part 3, I use an ASP.NET Core MVC website with the API from my Assignment2 project. The website sends requests to the API for account access, events, categories, enrolments, results and images. I kept the original event fields, including province, start time, entry fee and capacity. The original route and weather endpoints remain available through Swagger.

| Role | What I can do |
| --- | --- |
| Organiser | View my dashboard, create and edit events, upload banners, create and link categories, check enrolments and publish results after an event. I can delete an event with no enrolments. |
| Participant | Browse and filter upcoming events, choose a category, enrol, see the registration status, check results and update my profile and picture. |

I use the existing cookie-based API authentication and keep the API sign-in cookie in the MVC server session. The API checks the user's role and event ownership. For banner and profile uploads, the website sends the image to the API, which uses the Azure Blob Storage SDK. I use Azurite locally. An Azure Storage account can be used through the API configuration.

## How I run it with Docker Desktop

From my project folder containing `compose.yaml`, I run:

```powershell
docker compose up --build -d
docker compose ps
```

I open the [website](http://localhost:5090) and can inspect the API in [Swagger](http://localhost:5080/swagger). Compose starts MVC after the API health endpoint responds. The API stores application records in a JSON file in the `raceday-db` volume. Azurite stores images in its own volume. I stop both with `docker compose down`.

When I already have the API running and reachable from Docker Desktop on port 5080, I can run MVC on its own:

```powershell
docker build -t assignment2-raceday-mvc .
docker run --rm -p 127.0.0.1:5090:8080 -e RaceDayApi__BaseUrl=http://host.docker.internal:5080/ assignment2-raceday-mvc
```

## How I run it on Windows without Docker

I install the .NET 8 SDK and double-click `Start-RaceDay.cmd`. It starts the API, waits for the events endpoint, and opens MVC when it responds. I keep both application windows open. For local uploads, I start Azurite in a separate terminal with Node.js installed:

```powershell
npx --yes --package azurite@3.35.0 azurite-blob --location .raceday-blobs --disableTelemetry
```

I can use a real Azure Storage account with Compose by setting `RACEDAY_BLOB_CONNECTION` in a private local `.env` file. For an API run without a launch profile, I set `ConnectionStrings__RaceDayBlobs` in the API environment. I keep credentials out of the project files.

## Data and testing

The API still uses the Assignment2 in-memory store while it is running. The local launcher and Compose configure a JSON file to reload records after restart. The original SQL script remains in the API project as a schema reference. This version does not connect to SQL Server.

I can run the API controller tests with `dotnet test Assignment2.sln`. For a demo I register one Organiser and one Participant, create a category and future event, then enrol as the Participant. I move the event date to yesterday to demonstrate publishing a finish time and seeing it under **My Results**.
