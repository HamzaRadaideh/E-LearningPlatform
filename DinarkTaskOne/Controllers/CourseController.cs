using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DinarkTaskOne.Data;
using DinarkTaskOne.Models.ManageCourse;

namespace DinarkTaskOne.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class CourseController(ApplicationDbContext context) : Controller
    {
        // Helper methods to get the current user's ID and role
        private int GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("User ID claim is missing.");
            return int.Parse(userIdValue);
        }

        // 1. Create Course
        [HttpGet]
        public IActionResult CreateCourse()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse(CourseModel course)
        {
            var instructorId = GetCurrentUserId();

            // Check if the instructor is authorized
            var instructor = await context.Users
                .FirstOrDefaultAsync(u => u.UserId == instructorId && u.RoleId == 1 && u.UserType == "Instructor");

            if (instructor == null)
            {
                return View(course);
            }

            course.InstructorId = instructorId;
            context.Courses.Add(course);
            await context.SaveChangesAsync();

            return RedirectToAction("CourseConfig", new { id = course.CourseId });
        }

        // 2. Edit Course
        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await context.Courses.FindAsync(id);
            if (course == null || course.InstructorId != GetCurrentUserId())
            {
                return NotFound("Course not found or unauthorized access.");
            }

            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> EditCourse(CourseModel course)
        {
            var existingCourse = await context.Courses
                .Where(c => c.CourseId == course.CourseId && c.InstructorId == GetCurrentUserId())
                .FirstOrDefaultAsync();

            if (existingCourse == null)
            {
                return Unauthorized("Unauthorized access or course not found.");
            }

            existingCourse.Title = course.Title;
            existingCourse.Description = course.Description;
            context.Courses.Update(existingCourse);
            await context.SaveChangesAsync();

            return RedirectToAction("CourseConfig", new { id = course.CourseId });
        }

        // 3. Delete Course
        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            // Find the course
            var course = await context.Courses
                .Include(c => c.Enrollments) // Include related enrollments
                .FirstOrDefaultAsync(c => c.CourseId == id && c.InstructorId == GetCurrentUserId());

            if (course == null)
            {
                return NotFound("Course not found or unauthorized access.");
            }

            // Remove related enrollments first
            if (course.Enrollments.Any())
            {
                context.Enrollments.RemoveRange(course.Enrollments);
            }

            // Remove the course
            context.Courses.Remove(course);
            await context.SaveChangesAsync();

            return RedirectToAction("CoursesDashboard");
        }

        // 4. View Enrolled Students
        [HttpGet]
        public async Task<IActionResult> ViewStudents(int id)
        {
            var course = await context.Courses
                .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null || course.InstructorId != GetCurrentUserId())
            {
                return NotFound("Course not found or unauthorized access.");
            }

            return View(course.Enrollments);
        }

        // 5. Courses Dashboard
        [HttpGet]
        public async Task<IActionResult> CoursesDashboard()
        {
            var userId = GetCurrentUserId();
            var courses = await context.Courses
                .Where(c => c.InstructorId == userId)
                .ToListAsync();

            return View(courses);
        }

        // 6. Course Configuration
        public IActionResult CourseConfig(int id)
        {
            var course = context.Courses
                .Include(c => c.Materials)
                .Include(c => c.Announcements)
                .Include(c => c.Quizzes)
                .FirstOrDefault(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            // Combine materials, announcements, and quizzes into a single list and sort by CreatedAt
            var allContent = new List<dynamic>();

            allContent.AddRange(course.Materials.Select(m => new { Type = "Material", CreatedAt = m.CreatedAt, Content = m }));
            allContent.AddRange(course.Announcements.Select(a => new { Type = "Announcement", CreatedAt = a.CreatedAt, Content = a }));
            allContent.AddRange(course.Quizzes.Select(q => new { Type = "Quiz", CreatedAt = q.CreatedAt, Content = q }));

            // Sort by CreatedAt in descending order
            var sortedContent = allContent.OrderByDescending(c => c.CreatedAt).ToList();

            // Pass the sorted content to the view
            ViewBag.SortedContent = sortedContent;

            return View(course);
        }


    }
}
