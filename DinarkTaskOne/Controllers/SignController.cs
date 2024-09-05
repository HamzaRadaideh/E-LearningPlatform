using DinarkTaskOne.Models.Authentication_Authorization;
using DinarkTaskOne.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DinarkTaskOne.Controllers
{
    public class SignController(ISignService signService) : Controller
    {
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UsersModel user, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View(user);
            }

            user.RoleId = 2; // Assuming 2 is the RoleId for "Student" by default
            user.UserType = "Student"; // Set the UserType based on your logic

            await signService.RegisterUserAsync(user, password);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Email is required.");
                return View();
            }

            var user = await signService.GetUserByEmailAsync(email);
            if (user != null && signService.VerifyPassword(password, user.PasswordHash))
            {
                // Create claims for UserId, UserName, and RoleId
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.UserId.ToString()),  // UserId claim
                    new(ClaimTypes.Name, user.UserName),                     // UserName claim
                    new(ClaimTypes.Role, user.Role.RoleName),                // RoleName claim
                    new("RoleId", user.RoleId.ToString())                    // RoleId claim
                };

                var identity = new ClaimsIdentity(claims, "CustomScheme");
                var principal = new ClaimsPrincipal(identity);

                // Sign in the user with the custom scheme
                await HttpContext.SignInAsync("CustomScheme", principal);

                // Optionally store UserId and RoleId in session
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("RoleId", user.RoleId.ToString());

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout(string? returnUrl = null)
        {
            await HttpContext.SignOutAsync("CustomScheme");
            HttpContext.Session.Clear(); // Clear session data on logout
            return LocalRedirect(returnUrl ?? "/");
        }

        // Helper method to get the current user based on the stored UserId in session
#pragma warning disable IDE0051 // Remove unused private members
        private async Task<UsersModel?> GetCurrentUserAsync()
#pragma warning restore IDE0051 // Remove unused private members
        {
            var userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdString))
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Check if the claim is null before parsing
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    if (int.TryParse(userIdClaim, out int userId))
                    {
                        var user = await signService.GetUserByIdAsync(userId);
                        if (user != null)
                        {
                            HttpContext.Session.SetString("UserId", user.UserId.ToString());
                            HttpContext.Session.SetString("RoleId", user.RoleId.ToString());
                            return user;
                        }
                    }
                }
                return null;
            }
            else
            {
                // If the session already has a UserId, try to fetch the user based on that
                if (int.TryParse(userIdString, out int sessionId))
                {
                    return await signService.GetUserByIdAsync(sessionId);
                }
            }
            return null;
        }


        //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //var userName = User.Identity.Name; // UserName is stored here
        //var roleId = User.FindFirstValue("RoleId");

    }
}
