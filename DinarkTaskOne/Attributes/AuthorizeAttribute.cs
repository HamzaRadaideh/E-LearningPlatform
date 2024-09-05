using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using DinarkTaskOne.Services;
using System.Security.Claims;

namespace DinarkTaskOne.Attributes
{
#pragma warning disable CS9113 // Parameter is unread.
    public class AuthorizeAttribute(ISignService signService, params int[] roleIds) : ActionFilterAttribute
#pragma warning restore CS9113 // Parameter is unread.
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userIdString = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roleIdString = context.HttpContext.User.FindFirstValue("RoleId");

            if (string.IsNullOrEmpty(userIdString) || string.IsNullOrEmpty(roleIdString))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

#pragma warning disable IDE0059 // Unnecessary assignment of a value
            int userId = int.Parse(userIdString);
            int roleId = int.Parse(roleIdString);
#pragma warning restore IDE0059 // Unnecessary assignment of a value

            if (!roleIds.Contains(roleId))
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }

    }
}

//How to Use my Custom Authorization

// examples

// [TypeFilter(typeof(AuthorizeAttribute), Arguments = new object[] { "Admin", "Instructor" })]

// if (User.IsInRole("Instructor"))

// [TypeFilter(typeof(AuthorizeAttribute), Arguments = new object[] { "Instructor" })]

// [Authorize(Roles = "Instructor")]
