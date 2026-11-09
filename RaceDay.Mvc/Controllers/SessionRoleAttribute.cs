using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Mvc.Controllers;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SessionRoleAttribute(string role) : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        if (session.GetInt32("UserId") is null)
            context.Result = new RedirectToActionResult("Login", "Account", null);
        else if (session.GetString("Role") != role)
            context.Result = new ViewResult
            {
                ViewName = "~/Views/Shared/Error.cshtml",
                StatusCode = 403,
                ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
                    new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), context.ModelState)
                { Model = "Your account role does not have access to this page." }
            };
    }
}
