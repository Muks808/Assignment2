using Assignment2.Data;
using Assignment2.DTOs;
using Assignment2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Assignment2.Controllers
{
    [ApiController]
    [Route("api/results")]
    [Authorize(Roles = "Organiser")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayStore _store;
        public ResultsController(RaceDayStore store) => _store = store;
        private int CurrentUserID => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("enrollment/{enrollmentID:int}")]
        public IActionResult CreateResult(int enrollmentID, ResultDto dto)
        {
            var enrollment = _store.EventEnrollments.FirstOrDefault(e => e.EnrollmentID == enrollmentID);
            if (enrollment == null) return NotFound(new { message = "Enrolment does not exist." });
            var eventItem = _store.Events.FirstOrDefault(e => e.EventID == enrollment.EventID);
            var organiser = _store.Organisers.FirstOrDefault(o => o.UserID == CurrentUserID);
            if (eventItem == null || organiser == null || eventItem.OrganiserID != organiser.OrganiserID) return Forbid();
            if (eventItem.EventDate.Date >= DateTime.Today)
                return BadRequest(new { message = "Results open the day after the event." });
            if (_store.Results.Any(r => r.EnrollmentID == enrollmentID)) return Conflict(new { message = "Result already exists." });

            var result = new Result
            {
                ResultID = _store.GetNextResultId(),
                EnrollmentID = enrollmentID,
                FinishTime = dto.FinishTime,
                Position = dto.Position,
                AveragePace = dto.AveragePace.Trim(),
                ResultStatus = dto.ResultStatus.Trim()
            };
            _store.Results.Add(result);
            return StatusCode(201, result);
        }

        [HttpGet("event/{eventID:int}")]
        public IActionResult GetResults(int eventID)
        {
            var eventItem = _store.Events.FirstOrDefault(e => e.EventID == eventID);
            var organiser = _store.Organisers.FirstOrDefault(o => o.UserID == CurrentUserID);
            if (eventItem == null) return NotFound(new { message = "Event does not exist." });
            if (organiser == null || eventItem.OrganiserID != organiser.OrganiserID) return Forbid();

            var results = _store.Results.Where(r => _store.EventEnrollments.Any(e => e.EnrollmentID == r.EnrollmentID && e.EventID == eventID))
                .OrderBy(r => r.Position ?? int.MaxValue)
                .Select(r =>
                {
                    var enrollment = _store.EventEnrollments.First(e => e.EnrollmentID == r.EnrollmentID);
                    var participant = _store.Participants.First(p => p.ParticipantID == enrollment.ParticipantID);
                    var user = _store.Users.First(u => u.UserID == participant.UserID);
                    return new { r.ResultID, r.EnrollmentID, enrollment.ParticipantID, ParticipantName = $"{user.FirstName} {user.LastName}", r.FinishTime, r.Position, r.AveragePace, r.ResultStatus };
                });
            return Ok(results);
        }

        [HttpPut("{resultID:int}")]
        public IActionResult UpdateResult(int resultID, ResultDto dto)
        {
            var result = _store.Results.FirstOrDefault(r => r.ResultID == resultID);
            if (result == null) return NotFound(new { message = "Result does not exist." });
            var enrollment = _store.EventEnrollments.First(e => e.EnrollmentID == result.EnrollmentID);
            var eventItem = _store.Events.First(e => e.EventID == enrollment.EventID);
            var organiser = _store.Organisers.FirstOrDefault(o => o.UserID == CurrentUserID);
            if (organiser == null || eventItem.OrganiserID != organiser.OrganiserID) return Forbid();

            if (eventItem.EventDate.Date >= DateTime.Today)
                return BadRequest(new { message = "Results open the day after the event." });
            result.FinishTime = dto.FinishTime;
            result.Position = dto.Position;
            result.AveragePace = dto.AveragePace.Trim();
            result.ResultStatus = dto.ResultStatus.Trim();
            return Ok(result);
        }
    }
}
