using Microsoft.AspNetCore.Mvc;
using Assignment2.DTOs;

namespace RaceDay.Mvc.Controllers;

public sealed class AccountController(ApiClient api) : RaceDayController
{
    [HttpGet]
    public IActionResult Login() => View(new LoginDto());

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto request)
    {
        if (!ModelState.IsValid) return View(request);

        try
        {
            var session = HttpContext.Session;
            var response = await api.Read<Identity>(HttpMethod.Post, "api/auth/login", request);
            session.SetInt32("UserId", response.Id);
            session.SetString("Role", response.Role);
            session.SetString("FirstName", response.FirstName);
            return response.Role == "Organiser"
                ? RedirectToAction("Index", "Organiser") : RedirectToAction("Index", "Races");
        }
        catch (ApiFailure error)
        {
            ModelState.AddModelError(string.Empty, error.Message);
            return View(request);
        }
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterDto());

    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto request)
    {
        if (!ModelState.IsValid) return View(request);
        try
        {
            await api.Write(HttpMethod.Post, "api/auth/register", request);
            TempData["Success"] = "Account created. Sign in to continue.";
            return RedirectToAction(nameof(Login));
        }
        catch (ApiFailure error)
        {
            ModelState.AddModelError(string.Empty, error.Message);
            return View(request);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        try { await api.Write(HttpMethod.Post, "api/auth/logout"); }
        catch (ApiFailure) { }
        finally { HttpContext.Session.Clear(); }
        return RedirectToAction("Index", "Races");
    }
}
