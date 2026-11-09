using Microsoft.AspNetCore.Mvc;
using Assignment2.DTOs;

namespace RaceDay.Mvc.Controllers;

[SessionRole("Participant")]
public sealed class ParticipantController(ApiClient api) : RaceDayController
{
    [HttpGet]
    public async Task<IActionResult> Entries() => View(await api.Get<Entry[]>("api/enrolments"));

    [HttpPost]
    public async Task<IActionResult> Enter(int id, EnrolmentDto request)
    {
        await Save(() => api.Write(HttpMethod.Post, $"api/events/{id}/enrolments", request), "Entry registered.");
        return TempData.ContainsKey("Error") ? RedirectToAction("Details", "Races", new { id }) : RedirectToAction(nameof(Entries));
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        await Save(() => api.Write(HttpMethod.Delete, $"api/enrolments/{id}"), "Entry cancelled.");
        return RedirectToAction(nameof(Entries));
    }

    [HttpGet]
    public async Task<IActionResult> Results() => View(await api.Get<RaceResult[]>("api/users/me/results"));

    [HttpGet]
    public async Task<IActionResult> Profile() => View(await api.Get<Profile>("api/users/me"));

    [HttpPost, RequestSizeLimit(5_500_000)]
    public async Task<IActionResult> Profile(UpdateProfileDto request, IFormFile? picture)
    {
        await Save(async () =>
        {
            await api.Write(HttpMethod.Put, "api/users/me", request);
            if (picture is null) return;
            try { await api.Upload("api/users/me/image", picture); }
            catch (ApiFailure error)
            {
                throw new ApiFailure($"Profile details saved. Picture upload failed. {error.Message}", error.Status);
            }
        }, "Profile updated.");
        return RedirectToAction(nameof(Profile));
    }
}
