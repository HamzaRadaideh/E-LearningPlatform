using DinarkTaskOne.Models;
using DinarkTaskOne.Models.Authentication_Authorization;
using DinarkTaskOne.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DinarkTaskOne.Controllers
{
    [Authorize]
    public class AccountController(ISignService signService) : Controller
    {
        [HttpGet]
        public IActionResult Settings()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> UserInfo()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Sign");
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UserInfo(UsersModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Sign");
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.UserName = model.UserName;
            user.Email = model.Email;

            await signService.UpdateUserAsync(user);
            ViewBag.Message = "User info updated successfully!";

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> PersonalInfo()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Sign");
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> PersonalInfo(UsersModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Sign");
            }

            user.PhoneNumber = model.PhoneNumber;
            await signService.UpdateUserAsync(user);
            ViewBag.Message = "Personal info updated successfully!";

            return View(user);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePassword model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            if (user == null || !signService.VerifyPassword(model.OldPassword, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Old password is incorrect.");
                return View(model);
            }

            user.PasswordHash = signService.HashPassword(model.NewPassword);
            await signService.UpdateUserAsync(user);
            ViewBag.Message = "Password updated successfully!";

            return View();
        }


        private async Task<UsersModel?> GetCurrentUserAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            return await signService.GetUserByIdAsync(int.Parse(userId));
        }
    }
}
