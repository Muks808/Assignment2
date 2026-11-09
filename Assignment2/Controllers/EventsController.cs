using Assignment2.Data;
using Assignment2.DTOs;
using Assignment2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Assignment2.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayStore _store;
        public EventsController(RaceDayStore store) => _store = store;
        private int CurrentUserID => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetEvents() => Browse(null, null);

        [HttpGet("browse"), AllowAnonymous]
        public IActionResult Browse([FromQuery] string? type, [FromQuery] DateTime? fromDate)
        {
            var events = _store.Events.Where(e => (type == null || e.EventType == type) &&
                (!fromDate.HasValue || e.EventDate >= fromDate.Value)).OrderBy(e => e.EventDate).Select(e => new
            {
                e.EventID,
                e.OrganiserID,
                e.EventName, e.DistanceKm, e.EventType, e.BannerUrl,
                e.Description,
                e.EventDate,
                e.StartTime,
                e.Location,
                e.Province,
                e.EntryFee,
                e.MaxParticipants,
                e.Status,
                Categories = _store.EventCategories
                    .Where(ec => ec.EventID == e.EventID)
                    .Select(ec => _store.Categories.First(c => c.CategoryID == ec.CategoryID).CategoryName)
                    .ToList()
            });
            return Ok(events);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public IActionResult GetEvent(int id)
        {
            var e = _store.Events.FirstOrDefault(x => x.EventID == id);
            if (e == null) return NotFound(new { message = "Event does not exist." });
            return Ok(new
            {
                e.EventID,
                e.OrganiserID,
                e.EventName, e.DistanceKm, e.EventType, e.BannerUrl,
                e.Description,
                e.EventDate,
                e.StartTime,
                e.Location,
                e.Province,
                e.EntryFee,
                e.MaxParticipants,
                e.Status,
                CategoryOptions = Categories(id),
                Categories = _store.EventCategories.Where(x => x.EventID == id)
                    .Select(x => _store.Categories.First(c => c.CategoryID == x.CategoryID)),
                Route = _store.Routes.FirstOrDefault(r => r.EventID == id),
                Weather = _store.EventWeather.Where(w => w.EventID == id).ToList()
            });
        }

        private IEnumerable<Category> Categories(int id) => _store.EventCategories.Where(x => x.EventID == id)
            .Select(x => _store.Categories.First(c => c.CategoryID == x.CategoryID));

        [HttpGet("mine"), Authorize(Roles = "Organiser")]
        public IActionResult Mine()
        {
            var owner = _store.Organisers.FirstOrDefault(o => o.UserID == CurrentUserID);
            return Ok(_store.Events.Where(e => e.OrganiserID == owner!.OrganiserID).OrderBy(e => e.EventDate)
                .Select(e => new { e.EventID, e.EventName, e.Description, e.EventDate, e.StartTime, e.Location,
                    e.Province, e.EntryFee, e.MaxParticipants, e.Status, e.DistanceKm, e.EventType, e.BannerUrl,
                    EnrolmentCount = _store.EventEnrollments.Count(x => x.EventID == e.EventID) }));
        }

        [HttpPost("{id:int}/categories/{categoryId:int}"), Authorize(Roles = "Organiser")]
        public IActionResult AttachCategory(int id, int categoryId)
        {
            var race = _store.Events.FirstOrDefault(e => e.EventID == id);
            if (race is null || !_store.Categories.Any(c => c.CategoryID == categoryId)) return NotFound();
            if (!_store.Organisers.Any(o => o.UserID == CurrentUserID && o.OrganiserID == race.OrganiserID)) return Forbid();
            if (!_store.EventCategories.Any(c => c.EventID == id && c.CategoryID == categoryId))
                _store.EventCategories.Add(new EventCategory { EventID = id, CategoryID = categoryId });
            return NoContent();
        }

        [HttpGet("{id:int}/enrolments"), Authorize(Roles = "Organiser")]
        public IActionResult Entries(int id)
        {
            var race = _store.Events.FirstOrDefault(e => e.EventID == id);
            if (race is null) return NotFound();
            if (!_store.Organisers.Any(o => o.UserID == CurrentUserID && o.OrganiserID == race.OrganiserID)) return Forbid();
            return Ok(_store.EventEnrollments.Where(e => e.EventID == id).Select(e =>
            {
                var participant = _store.Participants.First(p => p.ParticipantID == e.ParticipantID);
                var user = _store.Users.First(u => u.UserID == participant.UserID);
                return new { e.EnrollmentID, e.Status, Participant = new { user.UserID, user.FirstName, user.LastName },
                    Category = _store.Categories.FirstOrDefault(c => c.CategoryID == e.CategoryID),
                    Result = _store.Results.FirstOrDefault(r => r.EnrollmentID == e.EnrollmentID) };
            }));
        }

        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public IActionResult CreateEvent(CreateEventDto dto)
        {
            if (dto.EventDate == default) return BadRequest(new { message = "Enter the event date." });
            var organiser = _store.Organisers.FirstOrDefault(o => o.UserID == CurrentUserID);
            if (organiser == null) return BadRequest(new { message = "Organiser profile not found." });
            if (!_store.Categories.Any(c => c.CategoryID == dto.CategoryID)) return BadRequest(new { message = "Category does not exist." });

            var e = new Event
            {
                EventID = _store.GetNextEventId(),
                OrganiserID = organiser.OrganiserID,
                EventName = dto.EventName.Trim(),
                DistanceKm = dto.DistanceKm, EventType = dto.EventType,
                Description = dto.Description.Trim(),
                EventDate = dto.EventDate,
                StartTime = dto.StartTime,
                Location = dto.Location.Trim(),
                Province = dto.Province.Trim(),
                EntryFee = dto.EntryFee,
                MaxParticipants = dto.MaxParticipants,
                Status = dto.Status.Trim()
            };
            _store.Events.Add(e);
            _store.EventCategories.Add(new EventCategory { EventID = e.EventID, CategoryID = dto.CategoryID });
            return CreatedAtAction(nameof(GetEvent), new { id = e.EventID }, e);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public IActionResult UpdateEvent(int id, CreateEventDto dto)
        {
            var e = _store.Events.FirstOrDefault(x => x.EventID == id);
            if (e == null) return NotFound(new { message = "Event does not exist." });
            var organiser = _store.Organisers.FirstOrDefault(o => o.UserID == CurrentUserID);
            if (organiser == null || e.OrganiserID != organiser.OrganiserID) return Forbid();
            if (!_store.Categories.Any(c => c.CategoryID == dto.CategoryID)) return BadRequest(new { message = "Category does not exist." });

            e.DistanceKm = dto.DistanceKm; e.EventType = dto.EventType;
            e.EventName = dto.EventName.Trim(); e.Description = dto.Description.Trim(); e.EventDate = dto.EventDate;
            e.StartTime = dto.StartTime; e.Location = dto.Location.Trim(); e.Province = dto.Province.Trim();
            e.EntryFee = dto.EntryFee; e.MaxParticipants = dto.MaxParticipants; e.Status = dto.Status.Trim();

            var existing = _store.EventCategories.FirstOrDefault(x => x.EventID == id);
            if (existing == null) _store.EventCategories.Add(new EventCategory { EventID = id, CategoryID = dto.CategoryID });
            else if (existing.CategoryID != dto.CategoryID)
            {
                var selected = _store.EventCategories.FirstOrDefault(x => x.EventID == id && x.CategoryID == dto.CategoryID);
                if (selected is not null) selected.CategoryID = existing.CategoryID;
                else if (_store.EventEnrollments.Any(x => x.EventID == id && x.CategoryID == existing.CategoryID))
                    _store.EventCategories.Add(new EventCategory { EventID = id, CategoryID = existing.CategoryID });
                existing.CategoryID = dto.CategoryID;
            }
            return Ok(e);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public IActionResult DeleteEvent(int id)
        {
            var e = _store.Events.FirstOrDefault(x => x.EventID == id);
            if (e == null) return NotFound(new { message = "Event does not exist." });
            var organiser = _store.Organisers.FirstOrDefault(o => o.UserID == CurrentUserID);
            if (organiser == null || e.OrganiserID != organiser.OrganiserID) return Forbid();
            if (_store.EventEnrollments.Any(x => x.EventID == id)) return BadRequest(new { message = "Event has enrolments and cannot be deleted." });
            _store.EventCategories.RemoveAll(x => x.EventID == id);
            _store.Routes.RemoveAll(x => x.EventID == id);
            _store.EventWeather.RemoveAll(x => x.EventID == id);
            _store.Events.Remove(e);
            return NoContent();
        }
    }
}