using Assignment2.Data;
using Assignment2.DTOs;
using Assignment2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Assignment2.Controllers
{
    [ApiController]
    [Route("api/weather")]
    public class EventWeatherController : ControllerBase
    {
        private readonly RaceDayStore _store;
        public EventWeatherController(RaceDayStore store) => _store = store;
        private int CurrentUserID => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("event/{eventID:int}")]
        [AllowAnonymous]
        public IActionResult GetForEvent(int eventID)
        {
            if (!_store.Events.Any(e => e.EventID == eventID)) return NotFound(new { message = "Event does not exist." });
            return Ok(_store.EventWeather.Where(w => w.EventID == eventID));
        }

        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public IActionResult CreateWeather(CreateWeatherDto dto)
        {
            var eventItem = _store.Events.FirstOrDefault(e => e.EventID == dto.EventID);
            var organiser = _store.Organisers.FirstOrDefault(o => o.UserID == CurrentUserID);
            if (eventItem == null) return NotFound(new { message = "Event does not exist." });
            if (organiser == null || eventItem.OrganiserID != organiser.OrganiserID) return Forbid();

            var weather = new EventWeather
            {
                WeatherID = _store.GetNextWeatherId(),
                EventID = dto.EventID,
                ForecastDate = dto.ForecastDate,
                TemperatureC = dto.TemperatureC,
                Condition = dto.Condition.Trim(),
                WindSpeedKmh = dto.WindSpeedKmh,
                RainChancePercent = dto.RainChancePercent,
                RecordedAt = DateTime.UtcNow
            };
            _store.EventWeather.Add(weather);
            return StatusCode(201, weather);
        }

        [HttpPut("{weatherID:int}")]
        [Authorize(Roles = "Organiser")]
        public IActionResult UpdateWeather(int weatherID, CreateWeatherDto dto)
        {
            var weather = _store.EventWeather.FirstOrDefault(w => w.WeatherID == weatherID);
            if (weather == null) return NotFound(new { message = "Weather record does not exist." });
            var eventItem = _store.Events.FirstOrDefault(e => e.EventID == weather.EventID);
            var organiser = _store.Organisers.FirstOrDefault(o => o.UserID == CurrentUserID);
            if (eventItem == null || organiser == null || eventItem.OrganiserID != organiser.OrganiserID) return Forbid();

            weather.ForecastDate = dto.ForecastDate; weather.TemperatureC = dto.TemperatureC; weather.Condition = dto.Condition.Trim();
            weather.WindSpeedKmh = dto.WindSpeedKmh; weather.RainChancePercent = dto.RainChancePercent; weather.RecordedAt = DateTime.UtcNow;
            return Ok(weather);
        }

        [HttpDelete("{weatherID:int}")]
        [Authorize(Roles = "Organiser")]
        public IActionResult DeleteWeather(int weatherID)
        {
            var weather = _store.EventWeather.FirstOrDefault(w => w.WeatherID == weatherID);
            if (weather == null) return NotFound(new { message = "Weather record does not exist." });
            var eventItem = _store.Events.FirstOrDefault(e => e.EventID == weather.EventID);
            var organiser = _store.Organisers.FirstOrDefault(o => o.UserID == CurrentUserID);
            if (eventItem == null || organiser == null || eventItem.OrganiserID != organiser.OrganiserID) return Forbid();
            _store.EventWeather.Remove(weather);
            return NoContent();
        }
    }
}