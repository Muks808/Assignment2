using Assignment2.Controllers;
using Assignment2.Data;
using Assignment2.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace Assignment2.Controllers
{
    [ApiController]
    [Route("api/routes")]
    public class RoutesController : ControllerBase
    {
        private readonly RaceDayStore _store;

        public RoutesController(RaceDayStore store)
        {
            _store = store;
        }

        private int CurrentUserID =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ============================================================
        // GET ALL ROUTES
        // ============================================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetRoutes()
        {
            return Ok(_store.Routes);
        }

        // ============================================================
        // GET ROUTE BY ID
        // ============================================================

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public IActionResult GetRoute(int id)
        {
            var route = _store.Routes
                .FirstOrDefault(r => r.RouteID == id);

            if (route == null)
            {
                return NotFound(new
                {
                    message = "Route does not exist."
                });
            }

            return Ok(route);
        }

        // ============================================================
        // CREATE ROUTE
        // ============================================================

        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public IActionResult CreateRoute(CreateRouteDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var eventItem = _store.Events
                .FirstOrDefault(e => e.EventID == dto.EventID);

            var organiser = _store.Organisers
                .FirstOrDefault(o => o.UserID == CurrentUserID);

            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event does not exist."
                });
            }

            if (organiser == null ||
                eventItem.OrganiserID != organiser.OrganiserID)
            {
                return Forbid();
            }

            if (_store.Routes.Any(r => r.EventID == dto.EventID))
            {
                return Conflict(new
                {
                    message = "This event already has a route."
                });
            }

            var route = new Assignment2.Models.Route
            {
                RouteID = _store.GetNextRouteId(),
                EventID = dto.EventID,
                RouteName = dto.RouteName.Trim(),
                DistanceKm = dto.DistanceKm,
                ElevationGainM = dto.ElevationGainM,
                StartPoint = dto.StartPoint.Trim(),
                FinishPoint = dto.FinishPoint.Trim(),
                RouteUrl = dto.RouteUrl?.Trim() ?? ""
            };

            _store.Routes.Add(route);

            return StatusCode(201, route);
        }

        // ============================================================
        // UPDATE ROUTE
        // ============================================================

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public IActionResult UpdateRoute(
            int id,
            CreateRouteDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var route = _store.Routes
                .FirstOrDefault(r => r.RouteID == id);

            if (route == null)
            {
                return NotFound(new
                {
                    message = "Route does not exist."
                });
            }

            var eventItem = _store.Events
                .FirstOrDefault(e => e.EventID == route.EventID);

            var organiser = _store.Organisers
                .FirstOrDefault(o => o.UserID == CurrentUserID);

            if (eventItem == null ||
                organiser == null ||
                eventItem.OrganiserID != organiser.OrganiserID)
            {
                return Forbid();
            }

            if (dto.EventID != route.EventID)
            {
                var newEvent = _store.Events
                    .FirstOrDefault(e => e.EventID == dto.EventID);

                if (newEvent == null)
                {
                    return NotFound(new
                    {
                        message = "The new event does not exist."
                    });
                }

                if (newEvent.OrganiserID != organiser.OrganiserID)
                {
                    return Forbid();
                }

                if (_store.Routes.Any(r =>
                    r.EventID == dto.EventID &&
                    r.RouteID != id))
                {
                    return Conflict(new
                    {
                        message = "The new event already has a route."
                    });
                }

                route.EventID = dto.EventID;
            }

            route.RouteName = dto.RouteName.Trim();
            route.DistanceKm = dto.DistanceKm;
            route.ElevationGainM = dto.ElevationGainM;
            route.StartPoint = dto.StartPoint.Trim();
            route.FinishPoint = dto.FinishPoint.Trim();
            route.RouteUrl = dto.RouteUrl?.Trim() ?? "";

            return Ok(route);
        }

        // ============================================================
        // DELETE ROUTE
        // ============================================================

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public IActionResult DeleteRoute(int id)
        {
            var route = _store.Routes
                .FirstOrDefault(r => r.RouteID == id);

            if (route == null)
            {
                return NotFound(new
                {
                    message = "Route does not exist."
                });
            }

            var eventItem = _store.Events
                .FirstOrDefault(e => e.EventID == route.EventID);

            var organiser = _store.Organisers
                .FirstOrDefault(o => o.UserID == CurrentUserID);

            if (eventItem == null ||
                organiser == null ||
                eventItem.OrganiserID != organiser.OrganiserID)
            {
                return Forbid();
            }

            _store.Routes.Remove(route);

            return NoContent();
        }
    }
}
