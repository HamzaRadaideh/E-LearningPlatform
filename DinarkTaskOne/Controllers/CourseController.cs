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
        private int GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("User ID claim is missing.");
            return int.Parse(userIdValue);
        }

        private int GetCurrentRoleId()
        {
            var roleIdValue = (User.FindFirst("RoleId")?.Value) ?? throw new InvalidOperationException("Role ID claim is missing.");
            return int.Parse(roleIdValue);
        }

        //private int GetCurrentUserId()
        //{
        //    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        //    return userId;
        //}
        //private int GetCurrentRoleId()
        //{
        //    return int.Parse(User.FindFirst("RoleId")?.Value ?? "0");
        //}

        // 1. Create Course
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CourseModel course)
        {
            var instructorId = GetCurrentUserId();

            // Query the Users table to find the instructor
            var instructor = await context.Users
                .FirstOrDefaultAsync(u => u.UserId == instructorId && u.RoleId == 1 && u.UserType == "Instructor");

            if (instructor == null)
            {
                Console.WriteLine("Instructor not found or unauthorized.");
                return View(course); // Return the view with an error
            }

            course.InstructorId = instructorId;
            context.Courses.Add(course);
            await context.SaveChangesAsync();

            Console.WriteLine($"Course added with ID: {course.CourseId}");

            return RedirectToAction("CourseConfig", new { id = course.CourseId });
        }

        // 2. Edit Course
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await context.Courses.FindAsync(id);
            if (course == null || course.InstructorId != GetCurrentUserId())
            {
                return NotFound("Course not found or unauthorized access.");
            }

            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CourseModel course)
        {
            var existingCourse = await context.Courses.FindAsync(course.CourseId);
            if (existingCourse == null || existingCourse.InstructorId != GetCurrentUserId())
            {
                return Unauthorized("Unauthorized access.");
            }

            existingCourse.Title = course.Title;
            existingCourse.Description = course.Description;

            context.Courses.Update(existingCourse);
            await context.SaveChangesAsync();

            return RedirectToAction("CourseConfig", new { id = course.CourseId });
        }

        // 3. Delete Course
        public async Task<IActionResult> Delete(int id)
        {
            var course = await context.Courses.FindAsync(id);
            if (course == null || course.InstructorId != GetCurrentUserId())
            {
                return NotFound("Course not found or unauthorized access.");
            }

            context.Courses.Remove(course);
            await context.SaveChangesAsync();

            return RedirectToAction("CoursesDashboard");
        }


        // 4. Upload Course Material
        [HttpGet]
        public IActionResult UploadMaterial(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadMaterial(int courseId, IFormFile file, string fileType, string? description)
        {
            var course = await context.Courses.FindAsync(courseId);
            if (course == null || course.InstructorId != GetCurrentUserId())
            {
                return NotFound("Course not found or unauthorized access.");
            }

            if (file != null && file.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");

                // Ensure the directory exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = file.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var material = new MaterialsModel
                {
                    CourseId = courseId,
                    FilePath = uniqueFileName, // Store only the unique file name
                    FileType = fileType,
                    Description = description
                };

                context.Materials.Add(material);
                await context.SaveChangesAsync();
            }

            return RedirectToAction("CourseConfig", new { id = courseId });
        }

        // 5. View Enrolled Students
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

        // 6. View all courses for the instructor (Dashboard)
        [HttpGet]
        public async Task<IActionResult> CoursesDashboard()
        {
            var userId = GetCurrentUserId();
            var courses = await context.Courses
                .Where(c => c.InstructorId == userId)
                .ToListAsync();

            return View(courses);
        }

        // 7. Configure a specific course (upload materials, create quizzes, view students)
        [HttpGet]
        public async Task<IActionResult> CourseConfig(int id)
        {
            var course = await context.Courses
                .Include(c => c.Materials)
                //.Include(c => c.Quizzes)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .Include(c => c.Announcements)
                .FirstOrDefaultAsync(c => c.CourseId == id && c.InstructorId == GetCurrentUserId());

            if (course == null)
            {
                return NotFound("Course not found or unauthorized access.");
            }

            return View(course);
        }

        // 8. Make an Announcement
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
            if (course == null || course.InstructorId != GetCurrentUserId())
            {
                return NotFound("Course not found or unauthorized access.");
            }

            var announcement = new AnnouncementModel
            {
                CourseId = courseId,
                Content = content,
                Type = type
            };

            context.Announcements.Add(announcement);
            await context.SaveChangesAsync();

            return RedirectToAction("CourseConfig", new { id = courseId });
        }
    }
}
