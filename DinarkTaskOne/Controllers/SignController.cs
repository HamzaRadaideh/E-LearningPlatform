using DinarkTaskOne.Data;
using DinarkTaskOne.Models.Authentication_Authorization;
using DinarkTaskOne.Models.ViewModels;
using DinarkTaskOne.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DinarkTaskOne.Controllers
{
    public class SignController(ISignService signService, ApplicationDbContext context) : Controller
    {
        [HttpGet]
        public IActionResult Register()
        {
            // Pass the departments and majors to the view
            ViewBag.Departments = context.Departments.ToList();
            ViewBag.Majors = context.Majors.ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Return the view with the current data if validation fails
                ViewBag.Departments = context.Departments.ToList();
                ViewBag.Majors = context.Majors.ToList();
                return View(model);
            }

            // Ensure passwords match
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                ViewBag.Departments = context.Departments.ToList();
                ViewBag.Majors = context.Majors.ToList();
                return View(model);
            }

            // Create a new user
            var user = new UsersModel
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                PasswordHash = signService.HashPassword(model.Password),
                RoleId = model.Role == "Instructor" ? 1 : 2,  // RoleId is 1 for Instructor and 2 for Student
                UserType = model.Role
            };

            // Assign DepartmentId or MajorId based on the role
            if (model.Role == "Instructor")
            {
                if (model.DepartmentId.HasValue)
                {
                    user.DepartmentId = model.DepartmentId;
                    user.MajorId = null; // Clear MajorId for instructors
                }
                else
                {
                    ModelState.AddModelError("", "Please select a valid Department for Instructors.");
                    ViewBag.Departments = context.Departments.ToList();
                    ViewBag.Majors = context.Majors.ToList();
                    return View(model);
                }
            }
            else if (model.Role == "Student")
            {
                if (model.MajorId.HasValue)
                {
                    user.MajorId = model.MajorId;
                    user.DepartmentId = null; // Clear DepartmentId for students
                }
                else
                {
                    ModelState.AddModelError("", "Please select a valid Major for Students.");
                    ViewBag.Departments = context.Departments.ToList();
                    ViewBag.Majors = context.Majors.ToList();
                    return View(model);
                }
            }

            // Register the user
            await signService.RegisterUserAsync(user, model.Password);

            // Redirect to login after successful registration
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
