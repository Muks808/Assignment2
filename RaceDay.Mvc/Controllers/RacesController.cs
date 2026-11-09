using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Mvc.Controllers;

public sealed class RacesController(ApiClient api) : RaceDayController
{
    [HttpGet]
    public async Task<IActionResult> Index(string? type, DateTime? fromDate)
    {
        var date = fromDate?.Date ?? DateTime.Today;
        if (date < DateTime.Today) date = DateTime.Today;
        var path = $"api/events/browse?fromDate={date:yyyy-MM-dd}";
        if (!string.IsNullOrWhiteSpace(type)) path += $"&type={Uri.EscapeDataString(type)}";
        return View(new RaceList(await api.Get<Race[]>(path), type, date));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id) => View(await api.Get<Race>($"api/events/{id}"));

    [HttpGet]
    public async Task<IActionResult> Image(string kind, int id)
    {
        if (kind != "banner" && (kind != "profile" || HttpContext.Session.GetInt32("UserId") != id))
            return NotFound();
        var image = await api.Image($"api/images/{kind}/{id}");
        Response.Headers["Cache-Control"] = "private, no-store";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(image.Bytes, image.ContentType);
    }

    public IActionResult Error() => View("~/Views/Shared/Error.cshtml", "Something went wrong. Please try again.");
}
