using DinarkTaskOne.Data;
using DinarkTaskOne.Models.ManageCourse;
using DinarkTaskOne.Models.ViewModels;
using DinarkTaskOne.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DinarkTaskOne.Controllers
{
    public class StudentController(IEnrollmentService enrollmentService, ApplicationDbContext context) : Controller
    {
        public async Task<IActionResult> MyCourses()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int studentId))
            {
                var enrollments = await enrollmentService.GetEnrollmentsByStudentIdAsync(studentId);
                return View(enrollments);
            }
            return Unauthorized(); // User ID not found or invalid
        }

        [HttpGet]
        public IActionResult Enroll(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EnrollPost(int courseId)
        {
            if (courseId <= 0)
            {
                ModelState.AddModelError("", "Please enter a valid Course ID.");
                ViewBag.CourseId = courseId;
                return View("Enroll");
            }

            var course = await context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
            {
                ModelState.AddModelError("", "The selected course does not exist.");
                ViewBag.CourseId = courseId;
                return View("Enroll");
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int studentId) && await enrollmentService.IsStudentEnrolledAsync(studentId, courseId))
            {
                ModelState.AddModelError("", "You are already enrolled in this course.");
                ViewBag.CourseId = courseId;
                return View("Enroll");
            }

            return View("EnrollPost", course);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmEnrollment(int courseId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int studentId))
            {
                var enrollmentSuccessful = await enrollmentService.EnrollStudentAsync(studentId, courseId);
                if (enrollmentSuccessful)
                {
                    ViewBag.Message = "Enrollment successful!";
                    return RedirectToAction("MyCourses");
                }
            }
            ModelState.AddModelError("", "An error occurred while trying to enroll.");
            return RedirectToAction("Enroll", new { courseId });
        }

        [HttpGet]
        public async Task<IActionResult> ViewCourse(int courseId)
        {
            var course = await context.Courses
                .Include(c => c.Announcements)
                .Include(c => c.Materials)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

#pragma warning disable CS8601 // Possible null reference assignment.
            var viewModel = new CourseViewModel
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                AnnouncementModelData = course.Announcements?.ToList() ?? [],
                MaterialsModelData = course.Materials?.ToList() ?? []
            };
#pragma warning restore CS8601 // Possible null reference assignment.

            return View(viewModel);
        }
    }
}
