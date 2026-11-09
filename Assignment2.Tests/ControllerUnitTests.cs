
using System.Security.Claims;
using Assignment2.Controllers;
using Assignment2.Data;
using Assignment2.DTOs;
using Assignment2.Models;
using Assignment2.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Assignment2.Tests;

public class ControllerUnitTests
{
    private static ClaimsPrincipal Principal(int id, string role) =>
        new(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.Name, "Test User")
        }, "UnitTest"));

    private static T AsUser<T>(T controller, int id, string role)
        where T : ControllerBase
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = Principal(id, role)
            }
        };

        return controller;
    }

    private static (
        RaceDayStore store,
        User user,
        Organiser organiser,
        Event eventItem,
        Category category,
        Participant participant
    ) Seed()
    {
        var store = new RaceDayStore();

        var user = new User
        {
            UserID = 1,
            FirstName = "Alex",
            LastName = "Runner",
            Email = "alex@example.com",
            Phone = "0711111111",
            Role = "Organiser"
        };

        var organiser = new Organiser
        {
            OrganiserID = 1,
            UserID = 1,
            OrganisationName = "RaceDay"
        };

        var participantUser = new User
        {
            UserID = 2,
            FirstName = "Sam",
            LastName = "Runner",
            Email = "sam@example.com",
            Phone = "0722222222",
            Role = "Participant"
        };

        var participant = new Participant
        {
            ParticipantID = 1,
            UserID = 2,
            Gender = "Other",
            EmergencyContact = "0722222222"
        };

        var category = new Category
        {
            CategoryID = 1,
            CategoryName = "Running",
            Description = "Running races"
        };

        var eventItem = new Event
        {
            EventID = 1,
            OrganiserID = 1,
            EventName = "City Run",
            Description = "A run",
            EventDate = new DateTime(2026, 11, 1),
            StartTime = new TimeSpan(8, 0, 0),
            Location = "Pretoria",
            Province = "Gauteng",
            EntryFee = 50,
            MaxParticipants = 20,
            Status = "Scheduled"
        };

        store.Users.AddRange(new[] { user, participantUser });
        store.Organisers.Add(organiser);
        store.Participants.Add(participant);
        store.Categories.Add(category);
        store.Events.Add(eventItem);

        store.EventCategories.Add(new EventCategory
        {
            EventID = 1,
            CategoryID = 1
        });

        return (
            store, user, organiser, eventItem, category, participant
        );
    }

    // CATEGORIES

    [Fact]
    public void Categories_GetAll_ReturnsOkAndCategories()
    {
        var (store, _, _, _, _, _) = Seed();

        var result = new CategoriesController(store).GetCategories();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Single(
            Assert.IsAssignableFrom<IEnumerable<Category>>(ok.Value));
    }

    [Fact]
    public void Categories_GetMissing_ReturnsNotFound()
    {
        var (store, _, _, _, _, _) = Seed();

        var result = new CategoriesController(store).GetCategory(99);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void Categories_Create_TrimsAndAddsCategory()
    {
        var (store, _, _, _, _, _) = Seed();

        var dto = new CreateCategoryDto
        {
            CategoryName = "  Cycling ",
            Description = " Ride "
        };

        var result = new CategoriesController(store).CreateCategory(dto);

        var created = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(201, created.StatusCode);

        Assert.Contains(store.Categories, c =>
            c.CategoryName == "Cycling" &&
            c.Description == "Ride");
    }

    [Fact]
    public void Categories_CreateDuplicate_ReturnsConflict()
    {
        var (store, _, _, _, _, _) = Seed();

        var dto = new CreateCategoryDto
        {
            CategoryName = " running ",
            Description = ""
        };

        var result = new CategoriesController(store).CreateCategory(dto);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public void Categories_DeleteUsedCategory_ReturnsBadRequest()
    {
        var (store, _, _, _, _, _) = Seed();

        var result = new CategoriesController(store).DeleteCategory(1);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // EVENTS

    [Fact]
    public void Events_GetMissing_ReturnsNotFound()
    {
        var (store, _, _, _, _, _) = Seed();

        var result = new EventsController(store).GetEvent(99);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void Events_Create_StoresEventAndCategory()
    {
        var (store, _, _, _, _, _) = Seed();

        var controller = AsUser(
            new EventsController(store), 1, "Organiser");

        var dto = new CreateEventDto
        {
            EventName = "  Park Run ",
            Description = " 5 km ",
            CategoryID = 1,
            EventDate = new DateTime(2026, 12, 1),
            StartTime = new TimeSpan(9, 0, 0),
            Location = " Pretoria ",
            Province = " Gauteng ",
            EntryFee = 0,
            MaxParticipants = 30,
            Status = " Scheduled "
        };

        var result = controller.CreateEvent(dto);

        Assert.IsType<CreatedAtActionResult>(result);
        Assert.Contains(store.Events, e =>
            e.EventName == "Park Run" &&
            e.Location == "Pretoria");
    }

    [Fact]
    public void Events_CreateWithUnknownCategory_ReturnsBadRequest()
    {
        var (store, _, _, _, _, _) = Seed();

        var controller = AsUser(
            new EventsController(store), 1, "Organiser");

        var result = controller.CreateEvent(
            new CreateEventDto { CategoryID = 99 });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Events_DeleteWithEnrolments_IsRejected()
    {
        var (store, _, _, _, _, _) = Seed();

        store.EventEnrollments.Add(new EventEnrollment
        {
            EnrollmentID = 1,
            EventID = 1,
            ParticipantID = 1
        });

        var controller = AsUser(
            new EventsController(store), 1, "Organiser");

        var result = controller.DeleteEvent(1);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Single(store.Events);
    }

    // ENROLMENTS

    [Fact]
    public void Enrolments_GetWithoutParticipantProfile_ReturnsNotFound()
    {
        var (store, _, _, _, _, _) = Seed();
        store.Participants.Clear();

        var controller = AsUser(
            new EnrolmentsController(store), 999, "Participant");

        var result = controller.GetMyEnrolments();

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void Enrolments_Enrol_GeneratesRaceNumberAndAddsRecord()
    {
        var (store, _, _, _, _, _) = Seed();
        store.Events.Single().EventDate = DateTime.Today.AddDays(1);

        var controller = AsUser(
            new EnrolmentsController(store), 2, "Participant");

        var result = controller.Enrol(
            1, new EnrolmentDto { Status = " Registered " });

        var created = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(201, created.StatusCode);
        Assert.Single(store.EventEnrollments);
        Assert.Equal("RD0001", store.EventEnrollments[0].RaceNumber);
        Assert.Equal("Registered", store.EventEnrollments[0].Status);
    }

    [Fact]
    public void Enrolments_EnrolTwice_ReturnsConflict()
    {
        var (store, _, _, _, _, _) = Seed();

        store.EventEnrollments.Add(new EventEnrollment
        {
            EnrollmentID = 1,
            EventID = 1,
            ParticipantID = 1
        });

        var controller = AsUser(
            new EnrolmentsController(store), 2, "Participant");

        var result = controller.Enrol(1, new EnrolmentDto());

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public void Enrolments_CancelMissing_ReturnsNotFound()
    {
        var (store, _, _, _, _, _) = Seed();

        var controller = AsUser(
            new EnrolmentsController(store), 2, "Participant");

        var result = controller.Cancel(44);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ROUTES

    [Fact]
    public void Routes_GetMissing_ReturnsNotFound()
    {
        var (store, _, _, _, _, _) = Seed();

        var result = new RoutesController(store).GetRoute(99);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void Routes_Create_AddsRoute()
    {
        var (store, _, _, _, _, _) = Seed();

        var dto = new CreateRouteDto
        {
            EventID = 1,
            RouteName = " City Loop ",
            DistanceKm = 5,
            ElevationGainM = 50,
            StartPoint = " Start ",
            FinishPoint = " Finish ",
            RouteUrl = " "
        };

        var controller = AsUser(
            new RoutesController(store), 1, "Organiser");

        var result = controller.CreateRoute(dto);

        Assert.Equal(201, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Contains(store.Routes, r =>
            r.RouteName == "City Loop" &&
            r.StartPoint == "Start" &&
            r.RouteUrl == "");
    }

    [Fact]
    public void Routes_CreateSecondRouteForEvent_ReturnsConflict()
    {
        var (store, _, _, _, _, _) = Seed();

        store.Routes.Add(new Assignment2.Models.Route
        {
            RouteID = 1,
            EventID = 1,
            RouteName = "Existing"
        });

        var dto = new CreateRouteDto
        {
            EventID = 1,
            RouteName = "New",
            DistanceKm = 5,
            StartPoint = "A",
            FinishPoint = "B"
        };

        var controller = AsUser(
            new RoutesController(store), 1, "Organiser");

        var result = controller.CreateRoute(dto);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public void Routes_DeleteRemovesRoute()
    {
        var (store, _, _, _, _, _) = Seed();

        store.Routes.Add(new Assignment2.Models.Route
        {
            RouteID = 1,
            EventID = 1,
            RouteName = "Loop"
        });

        var controller = AsUser(
            new RoutesController(store), 1, "Organiser");

        var result = controller.DeleteRoute(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(store.Routes);
    }

    // EVENT WEATHER

    [Fact]
    public void Weather_GetForMissingEvent_ReturnsNotFound()
    {
        var (store, _, _, _, _, _) = Seed();

        var result = new EventWeatherController(store).GetForEvent(99);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void Weather_Create_AddsForecast()
    {
        var (store, _, _, _, _, _) = Seed();

        var dto = new CreateWeatherDto
        {
            EventID = 1,
            ForecastDate = new DateTime(2026, 11, 1),
            Condition = " Sunny ",
            TemperatureC = 22
        };

        var controller = AsUser(
            new EventWeatherController(store), 1, "Organiser");

        var result = controller.CreateWeather(dto);

        Assert.Equal(201, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Single(store.EventWeather);
        Assert.Equal("Sunny", store.EventWeather[0].Condition);
    }

    [Fact]
    public void Weather_DeleteMissing_ReturnsNotFound()
    {
        var (store, _, _, _, _, _) = Seed();

        var controller = AsUser(
            new EventWeatherController(store), 1, "Organiser");

        var result = controller.DeleteWeather(99);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // RESULTS

    [Fact]
    public void Results_CreateMissingEnrollment_ReturnsNotFound()
    {
        var (store, _, _, _, _, _) = Seed();

        var controller = AsUser(
            new ResultsController(store), 1, "Organiser");

        var result = controller.CreateResult(99, new ResultDto());

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void Results_CreateDuplicate_ReturnsConflict()
    {
        var (store, _, _, _, _, _) = Seed();

        store.EventEnrollments.Add(new EventEnrollment
        {
            EnrollmentID = 1,
            EventID = 1,
            ParticipantID = 1
        });

        store.Results.Add(new Result
        {
            ResultID = 1,
            EnrollmentID = 1
        });

        var controller = AsUser(
            new ResultsController(store), 1, "Organiser");

        var result = controller.CreateResult(1, new ResultDto());

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public void Results_CreateAddsResult()
    {
        var (store, _, _, _, _, _) = Seed();
        store.Events.Single().EventDate = DateTime.Today.AddDays(-1);

        store.EventEnrollments.Add(new EventEnrollment
        {
            EnrollmentID = 1,
            EventID = 1,
            ParticipantID = 1
        });

        var dto = new ResultDto
        {
            AveragePace = " 5:30 ",
            ResultStatus = " Finished "
        };

        var controller = AsUser(
            new ResultsController(store), 1, "Organiser");

        var result = controller.CreateResult(1, dto);

        Assert.Equal(201, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Single(store.Results);
        Assert.Equal("5:30", store.Results[0].AveragePace);
    }

    // USERS

    [Fact]
    public void Users_ProfileForMissingUser_ReturnsUnauthorized()
    {
        var (store, _, _, _, _, _) = Seed();

        var controller = AsUser(
            new UsersController(store), 999, "Participant");

        var result = controller.GetProfile();

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void Users_UpdateProfileNormalizesEmailAndTrimsNames()
    {
        var (store, _, _, _, _, _) = Seed();

        var dto = new UpdateProfileDto
        {
            FirstName = " Alex ",
            LastName = " New ",
            Email = " ALEX@EXAMPLE.COM ",
            Phone = " 0712345678 "
        };

        var controller = AsUser(
            new UsersController(store), 1, "Organiser");

        var result = controller.UpdateProfile(dto);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Alex", store.Users[0].FirstName);
        Assert.Equal("alex@example.com", store.Users[0].Email);
        Assert.Equal("0712345678", store.Users[0].Phone);
    }

    [Fact]
    public void Users_UpdateProfileDuplicateEmail_ReturnsConflict()
    {
        var (store, _, _, _, _, _) = Seed();

        var dto = new UpdateProfileDto
        {
            FirstName = "Alex",
            LastName = "Runner",
            Email = "SAM@example.com",
            Phone = "0711111111"
        };

        var controller = AsUser(
            new UsersController(store), 1, "Organiser");

        var result = controller.UpdateProfile(dto);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public void Users_MyResultsWithoutParticipantProfile_ReturnsNotFound()
    {
        var (store, _, _, _, _, _) = Seed();
        store.Participants.Clear();

        var controller = AsUser(
            new UsersController(store), 2, "Participant");

        var result = controller.MyResults();

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // AUTHENTICATION

    [Fact]
    public void Auth_RegisterCreatesParticipantAccount()
    {
        var store = new RaceDayStore();
        var service = new AuthService(store, new PasswordService());
        var controller = new AuthController(service);

        var dto = new RegisterDto
        {
            FirstName = "Taylor",
            LastName = "Tester",
            Email = "TAYLOR@example.com",
            Password = "Password123!",
            Phone = "0712345678",
            Role = "Participant"
        };

        var result = controller.Register(dto);

        Assert.Equal(201, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
        Assert.Single(store.Users);
        Assert.Single(store.Participants);
        Assert.Equal("taylor@example.com", store.Users[0].Email);
    }

    [Fact]
    public void Auth_RegisterDuplicateEmailReturnsConflict()
    {
        var store = new RaceDayStore();

        store.Users.Add(new User
        {
            UserID = 1,
            Email = "same@example.com"
        });

        var controller = new AuthController(
            new AuthService(store, new PasswordService()));

        var dto = new RegisterDto
        {
            FirstName = "A",
            LastName = "B",
            Email = "SAME@example.com",
            Password = "Password123!",
            Phone = "0712345678",
            Role = "Participant"
        };

        var result = controller.Register(dto);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public void Auth_ForbiddenEndpointReturns403()
    {
        var controller = new AuthController(
            new AuthService(new RaceDayStore(), new PasswordService()));

        var result = controller.Forbidden();

        Assert.Equal(403, Assert.IsAssignableFrom<ObjectResult>(result).StatusCode);
    }

    // WEATHER FORECAST

    [Fact]
    public void WeatherForecast_ReturnsFiveForecasts()
    {
        var controller = new WeatherForecastController(
            NullLogger<WeatherForecastController>.Instance);

        var result = controller.Get().ToArray();

        Assert.Equal(5, result.Length);
        Assert.All(result, item =>
            Assert.False(string.IsNullOrWhiteSpace(item.Summary)));
    }
}
