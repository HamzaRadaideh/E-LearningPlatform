using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DinarkTaskOne.Data;
using DinarkTaskOne.Models.ManageCourse;
using System.Security.Claims;

namespace DinarkTaskOne.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class AnnounceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnnounceController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("User ID claim is missing.");
            return int.Parse(userIdValue);
        }

        // 1. Make an Announcement - GET
        [HttpGet]
        public IActionResult MakeAnnouncement(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        // 1. Make an Announcement - POST
        [HttpPost]
        public async Task<IActionResult> MakeAnnouncement(int courseId, string content, string type)
        {
            var instructorId = GetCurrentUserId();
            var course = await _context.Courses
                .Where(c => c.CourseId == courseId && c.InstructorId == instructorId)
                .FirstOrDefaultAsync();

            if (course == null)
            {
                return NotFound("Course not found or unauthorized access.");
            }

            var announcement = new AnnouncementModel
            {
                CourseId = courseId,
                Content = content,
                Type = type,
                CreatedAt = DateTime.UtcNow
            };

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            return RedirectToAction("CourseConfig", "Course", new { id = courseId });
        }

        // 2. Edit Announcement - GET
        [HttpGet]
        public async Task<IActionResult> EditAnnouncement(int id)
        {
            var announcement = await _context.Announcements
                .Include(a => a.Course)
                .Where(a => a.AnnouncementId == id && a.Course.InstructorId == GetCurrentUserId())
                .FirstOrDefaultAsync();

            if (announcement == null)
            {
                return NotFound("Announcement not found or unauthorized access.");
            }

            return View(announcement);
        }

        // 2. Edit Announcement - POST
        [HttpPost]
        public async Task<IActionResult> EditAnnouncement(AnnouncementModel announcement)
        {
            var existingAnnouncement = await _context.Announcements
                .Include(a => a.Course)
                .Where(a => a.AnnouncementId == announcement.AnnouncementId && a.Course.InstructorId == GetCurrentUserId())
                .FirstOrDefaultAsync();

            if (existingAnnouncement == null)
            {
                return NotFound("Announcement not found or unauthorized access.");
            }

            existingAnnouncement.Content = announcement.Content;
            existingAnnouncement.Type = announcement.Type;
            existingAnnouncement.UpdatedAt = DateTime.UtcNow;

            _context.Announcements.Update(existingAnnouncement);
            await _context.SaveChangesAsync();

            return RedirectToAction("CourseConfig", "Course", new { id = existingAnnouncement.CourseId });
        }

        // 3. Delete Announcement
        [HttpPost]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            var announcement = await _context.Announcements
                .Include(a => a.Course)
                .Where(a => a.AnnouncementId == id && a.Course.InstructorId == GetCurrentUserId())
                .FirstOrDefaultAsync();

            if (announcement == null)
            {
                return NotFound("Announcement not found or unauthorized access.");
            }

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();

            return RedirectToAction("CourseConfig", "Course", new { id = announcement.CourseId });
        }
    }
}
