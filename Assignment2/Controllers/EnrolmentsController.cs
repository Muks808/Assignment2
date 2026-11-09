using Assignment2.Data;
using Assignment2.DTOs;
using Assignment2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Assignment2.Controllers
{
    [ApiController]
    [Route("api/enrolments")]
    [Authorize(Roles = "Participant")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayStore _store;
        public EnrolmentsController(RaceDayStore store) => _store = store;
        private int CurrentUserID => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private Participant? CurrentParticipant => _store.Participants.FirstOrDefault(p => p.UserID == CurrentUserID);

        [HttpGet]
        public IActionResult GetMyEnrolments()
        {
            var participant = CurrentParticipant;
            if (participant == null) return NotFound(new { message = "Participant profile not found." });

            return Ok(_store.EventEnrollments
                .Where(e => e.ParticipantID == participant.ParticipantID)
                .Select(e =>
                {
                    var eventItem = _store.Events.First(x => x.EventID == e.EventID);
                    return new
                    {
                        e.EnrollmentID,
                        Event = eventItem,
                        Category = _store.Categories.FirstOrDefault(c => c.CategoryID == e.CategoryID),
                        e.EventID,
                        EventName = eventItem.EventName,
                        eventItem.EventDate,
                        eventItem.StartTime,
                        eventItem.Location,
                        e.RaceNumber,
                        e.Status,
                        e.EnrolmentDate
                    };
                }));
        }

        [HttpPost("/api/events/{eventId:int}/enrolments")]
        public IActionResult Enrol(int eventId, EnrolmentDto dto)
        {
            var participant = CurrentParticipant;
            if (participant == null) return BadRequest(new { message = "Participant profile not found." });
            var eventItem = _store.Events.FirstOrDefault(e => e.EventID == eventId);
            if (eventItem == null) return NotFound(new { message = "Event does not exist." });
            if (_store.EventEnrollments.Any(e => e.EventID == eventId && e.ParticipantID == participant.ParticipantID))
                return Conflict(new { message = "You are already enrolled in this event." });

            if (eventItem.MaxParticipants.HasValue && _store.EventEnrollments.Count(e => e.EventID == eventId) >= eventItem.MaxParticipants.Value)
                return BadRequest(new { message = "This event is full." });

            if (eventItem.EventDate.Date < DateTime.Today)
                return BadRequest(new { message = "Entries are closed for past events." });
            var categoryId = dto.CategoryID ?? _store.EventCategories.FirstOrDefault(c => c.EventID == eventId)?.CategoryID;
            if (categoryId.HasValue && !_store.EventCategories.Any(c => c.EventID == eventId && c.CategoryID == categoryId))
                return BadRequest(new { message = "Choose a category for this event." });

            var enrollmentID = _store.GetNextEnrollmentId();
            var enrollment = new EventEnrollment
            {
                EnrollmentID = enrollmentID,
                EventID = eventId,
                ParticipantID = participant.ParticipantID,
                CategoryID = categoryId,
                EnrolmentDate = DateTime.UtcNow,
                RaceNumber = string.IsNullOrWhiteSpace(dto.RaceNumber) ? $"RD{enrollmentID:D4}" : dto.RaceNumber.Trim(),
                Status = dto.Status.Trim()
            };
            _store.EventEnrollments.Add(enrollment);
            return StatusCode(201, enrollment);
        }

        [HttpDelete("{enrollmentID:int}")]
        public IActionResult Cancel(int enrollmentID)
        {
            var participant = CurrentParticipant;
            if (participant == null) return BadRequest(new { message = "Participant profile not found." });
            var enrollment = _store.EventEnrollments.FirstOrDefault(e => e.EnrollmentID == enrollmentID && e.ParticipantID == participant.ParticipantID);
            if (enrollment == null) return NotFound(new { message = "Enrolment does not exist." });
            if (_store.Results.Any(r => r.EnrollmentID == enrollmentID)) return BadRequest(new { message = "An enrolment with a result cannot be cancelled." });
            _store.EventEnrollments.Remove(enrollment);
            return NoContent();
        }
    }
}
