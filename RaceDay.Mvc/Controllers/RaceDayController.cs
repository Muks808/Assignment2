using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Mvc.Controllers;

public abstract class RaceDayController : Controller
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();
        if (executed.Exception is ApiFailure failure && !executed.ExceptionHandled)
        {
            executed.ExceptionHandled = true;
            if (failure.Status == HttpStatusCode.Unauthorized)
                executed.Result = RedirectToAction("Login", "Account");
            else
            {
                Response.StatusCode = (int)failure.Status;
                executed.Result = View("~/Views/Shared/Error.cshtml", failure.Message);
            }
        }
    }

    protected async Task Save(Func<Task> operation, string success)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = string.Join(" ", ModelState.Values.SelectMany(value => value.Errors)
                .Select(error => string.IsNullOrEmpty(error.ErrorMessage) ? "Check the entered values." : error.ErrorMessage));
            return;
        }

        try
        {
            await operation();
            TempData["Success"] = success;
        }
        catch (ApiFailure error)
        {
            TempData["Error"] = error.Message;
        }
    }
}
