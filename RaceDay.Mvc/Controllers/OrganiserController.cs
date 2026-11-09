using Microsoft.AspNetCore.Mvc;
using Assignment2.DTOs;

namespace RaceDay.Mvc.Controllers;

[SessionRole("Organiser")]
public sealed class OrganiserController(ApiClient api) : RaceDayController
{
    [HttpGet]
    public async Task<IActionResult> Index() => View(await api.Get<Race[]>("api/events/mine"));

    [HttpGet]
    public async Task<IActionResult> Edit(int id = 0)
    {
        var categories = await api.Get<Category[]>("api/categories");
        if (id == 0) return View(new RaceEditor(0, new CreateEventDto
        {
            EventDate = DateTime.Today.AddDays(1), StartTime = TimeSpan.FromHours(8)
        }, null, categories));
        var race = (await api.Get<Race[]>("api/events/mine")).SingleOrDefault(item => item.Id == id);
        if (race is null) return NotFound();

        var fields = new CreateEventDto
        {
            EventName = race.Name, Description = race.Description, EventDate = race.EventDate,
            StartTime = race.StartTime, Province = race.Province, EntryFee = race.EntryFee,
            MaxParticipants = race.MaxParticipants, Status = race.Status,
            CategoryID = (await api.Get<Race>($"api/events/{id}")).Categories?.FirstOrDefault()?.Id ?? 0,
            Location = race.Location, DistanceKm = race.DistanceKm, EventType = race.EventType
        };
        return View(new RaceEditor(id, fields, race.BannerUrl, categories));
    }

    [HttpPost, RequestSizeLimit(5_500_000)]
    public async Task<IActionResult> Edit(int id, [Bind(Prefix = "Fields")] CreateEventDto fields, IFormFile? banner)
    {
        if (!ModelState.IsValid) return View(new RaceEditor(id, fields, null, await api.Get<Category[]>("api/categories")));

        try
        {
            var saved = id == 0
                ? await api.Read<Race>(HttpMethod.Post, "api/events", fields)
                : await api.Read<Race>(HttpMethod.Put, $"api/events/{id}", fields);
            id = saved.Id;
        }
        catch (ApiFailure error)
        {
            ModelState.AddModelError(string.Empty, error.Message);
            return View(new RaceEditor(id, fields, null, await api.Get<Category[]>("api/categories")));
        }

        if (banner is not null)
        {
            try { await api.Upload($"api/events/{id}/banner", banner); }
            catch (ApiFailure error)
            {
                TempData["Error"] = $"Event saved. Banner upload failed. {error.Message}";
                return RedirectToAction(nameof(Edit), new { id });
            }
        }
        TempData["Success"] = "Event saved.";
        return RedirectToAction(nameof(Manage), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Manage(int id)
    {
        var entries = await api.Get<Entry[]>($"api/events/{id}/enrolments");
        return View(new ManageRace(await api.Get<Race>($"api/events/{id}"), entries, await api.Get<Category[]>("api/categories")));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await Save(() => api.Write(HttpMethod.Delete, $"api/events/{id}"), "Event deleted.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> AddCategory(int id, CreateCategoryDto request)
    {
        await Save(() => api.Write(HttpMethod.Post, $"api/categories", request), "Category added.");
        return id == 0 ? RedirectToAction(nameof(Edit)) : RedirectToAction(nameof(Manage), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> AttachCategory(int id, int categoryId)
    {
        await Save(() => api.Write(HttpMethod.Post, $"api/events/{id}/categories/{categoryId}"), "Category linked.");
        return RedirectToAction(nameof(Manage), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Publish(int id, int entryId, int? resultId, ResultDto request)
    {
        await Save(() => resultId.HasValue
            ? api.Write(HttpMethod.Put, $"api/results/{resultId}", request)
            : api.Write(HttpMethod.Post, $"api/results/enrollment/{entryId}", request), "Result published.");
        return RedirectToAction(nameof(Manage), new { id });
    }
}
