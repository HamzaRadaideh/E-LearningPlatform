using DinarkTaskOne.Data;
using DinarkTaskOne.Models.Authentication_Authorization;
using DinarkTaskOne.Models.UserSpecficModels;
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
                ViewBag.Departments = context.Departments.ToList();
                ViewBag.Majors = context.Majors.ToList();

                // Handle AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_RegisterFormPartial", model);
                }

                return View(model);
            }

            // Ensure passwords match
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                ViewBag.Departments = context.Departments.ToList();
                ViewBag.Majors = context.Majors.ToList();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_RegisterFormPartial", model);
                }

                return View(model);
            }

            UsersModel user;

            if (model.Role == "Instructor")
            {
                if (!context.Departments.Any(d => d.DepartmentId == model.DepartmentId))
                {
                    ModelState.AddModelError("", "Invalid Department selected.");
                    ViewBag.Departments = context.Departments.ToList();
                    ViewBag.Majors = context.Majors.ToList();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return PartialView("_RegisterFormPartial", model);
                    }

                    return View(model);
                }

                user = new InstructorModel
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    PasswordHash = signService.HashPassword(model.Password),
                    RoleId = 1,  // RoleId for Instructor
                    UserType = model.Role,
                    DepartmentId = model.DepartmentId.Value
                };
            }
            else if (model.Role == "Student")
            {
                if (!context.Majors.Any(m => m.MajorId == model.MajorId))
                {
                    ModelState.AddModelError("", "Invalid Major selected.");
                    ViewBag.Departments = context.Departments.ToList();
                    ViewBag.Majors = context.Majors.ToList();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return PartialView("_RegisterFormPartial", model);
                    }

                    return View(model);
                }

                user = new StudentModel
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    PasswordHash = signService.HashPassword(model.Password),
                    RoleId = 2,  // RoleId for Student
                    UserType = model.Role,
                    MajorId = model.MajorId.Value,
                    CurrentLevelId = 1 // Default level for new students
                };
            }
            else
            {
                ModelState.AddModelError("", "Invalid role selected.");
                ViewBag.Departments = context.Departments.ToList();
                ViewBag.Majors = context.Majors.ToList();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_RegisterFormPartial", model);
                }

                return View(model);
            }

            await signService.RegisterUserAsync(user, model.Password);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

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

                //if (user is InstructorModel instructor)
                //{
                //    claims.Add(new Claim("InstructorId", instructor.InstructorId.ToString()));
                //}
                //else if (user is StudentModel student)
                //{
                //    claims.Add(new Claim("StudentId", student.StudentId.ToString()));
                //}

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
        private async Task<UsersModel?> GetCurrentUserAsync()
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
