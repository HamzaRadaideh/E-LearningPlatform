using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DinarkTaskOne.Data;
using DinarkTaskOne.Models.ManageCourse;
using System.Security.Claims;

namespace DinarkTaskOne.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class AnnounceController(ApplicationDbContext context) : Controller
    {

        private int GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("User ID claim is missing.");
            return int.Parse(userIdValue);
        }

        // 1. Make an Announcement
        [HttpGet]
        public IActionResult MakeAnnouncement(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> MakeAnnouncement(int courseId, string content, string type)
        {
            var course = await context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return NotFound("Course not found.");
            }

            var announcement = new AnnouncementModel
            {
                CourseId = courseId,
                Content = content,
                Type = type
            };

            context.Announcements.Add(announcement);
            await context.SaveChangesAsync();

            return RedirectToAction("CourseConfig", "Course", new { id = courseId });
        }

        // 2. Edit Announcement
        [HttpGet]
        public async Task<IActionResult> EditAnnouncement(int id)
        {
            var announcement = await context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                return NotFound("Announcement not found.");
            }

            return View(announcement);
        }

        [HttpPost]
        public async Task<IActionResult> EditAnnouncement(AnnouncementModel announcement)
        {
            var existingAnnouncement = await context.Announcements
                .Include(a => a.Course)
                .Where(a => a.AnnouncementId == announcement.AnnouncementId && a.Course.InstructorId == GetCurrentUserId())
                .FirstOrDefaultAsync();

            if (existingAnnouncement == null)
            {
                return NotFound("Announcement not found or unauthorized access.");
            }

            existingAnnouncement.Content = announcement.Content;
            existingAnnouncement.Type = announcement.Type;
            context.Announcements.Update(existingAnnouncement);
            await context.SaveChangesAsync();

            return RedirectToAction("CourseConfig", "Course", new { id = existingAnnouncement.CourseId });
        }


        // 3. Delete Announcement
        [HttpPost]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            var announcement = await context.Announcements
                .Include(a => a.Course)
                .Where(a => a.AnnouncementId == id && a.Course.InstructorId == GetCurrentUserId())
                .FirstOrDefaultAsync();

            if (announcement == null)
            {
                return NotFound("Announcement not found or unauthorized access.");
            }

            context.Announcements.Remove(announcement);
            await context.SaveChangesAsync();

            return RedirectToAction("CourseConfig", "Course", new { id = announcement.CourseId });
        }
    }
}
