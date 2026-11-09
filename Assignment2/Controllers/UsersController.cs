using Assignment2.Data;
using Assignment2.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Assignment2.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayStore _store;
        public UsersController(RaceDayStore store) => _store = store;

        private int CurrentUserID => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("me")]
        public IActionResult GetProfile()
        {
            var user = _store.Users.FirstOrDefault(u => u.UserID == CurrentUserID);
            if (user == null) return Unauthorized();

            return Ok(new
            {
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Phone,
                user.Role,
                user.ProfileImageUrl,
                user.CreatedAt
            });
        }

        [HttpPut("me")]
        public IActionResult UpdateProfile(UpdateProfileDto dto)
        {
            var user = _store.Users.FirstOrDefault(u => u.UserID == CurrentUserID);
            if (user == null) return Unauthorized();

            var email = dto.Email.Trim().ToLowerInvariant();
            if (_store.Users.Any(u => u.Email == email && u.UserID != CurrentUserID))
                return Conflict(new { message = "Email already exists." });

            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.Email = email;
            user.Phone = dto.Phone.Trim();

            return Ok(new
            {
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Phone,
                user.Role
            });
        }

        [HttpGet]
        [Authorize(Roles = "Organiser")]
        public IActionResult GetUsers()
        {
            return Ok(_store.Users.Select(u => new
            {
                u.UserID,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Phone,
                u.Role,
                u.CreatedAt
            }));
        }

        [HttpGet("me/results")]
        [Authorize(Roles = "Participant")]
        public IActionResult MyResults()
        {
            var participant = _store.Participants.FirstOrDefault(p => p.UserID == CurrentUserID);
            if (participant == null) return NotFound(new { message = "Participant profile not found." });

            var results = _store.Results
                .Where(r => _store.EventEnrollments.Any(e => e.EnrollmentID == r.EnrollmentID && e.ParticipantID == participant.ParticipantID))
                .Select(r =>
                {
                    var enrollment = _store.EventEnrollments.First(e => e.EnrollmentID == r.EnrollmentID);
                    var eventItem = _store.Events.FirstOrDefault(e => e.EventID == enrollment.EventID);
                    return new
                    {
                        r.ResultID,
                        enrollment.EnrollmentID,
                        enrollment.EventID,
                        EventName = eventItem?.EventName,
                        Event = eventItem,
                        Category = _store.Categories.FirstOrDefault(c => c.CategoryID == enrollment.CategoryID)?.CategoryName,
                        TotalFinishers = _store.Results.Count(other => other.ResultStatus == "Finished" &&
                            _store.EventEnrollments.Any(entry => entry.EventID == enrollment.EventID && entry.EnrollmentID == other.EnrollmentID)),
                        r.FinishTime,
                        r.Position,
                        r.AveragePace,
                        r.ResultStatus
                    };
                });

            return Ok(results);
        }
    }
}
