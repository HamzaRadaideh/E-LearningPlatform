using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DinarkTaskOne.Data;
using DinarkTaskOne.Models.ManageCourse;
using Microsoft.AspNetCore.Mvc.Rendering;
using DinarkTaskOne.Models.ViewModels;
using System.Linq;

namespace DinarkTaskOne.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper method to get the current user's ID
        private int GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("User ID claim is missing.");
            return int.Parse(userIdValue);
        }

        // 1. Create Course - GET
        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
            var instructorId = GetCurrentUserId();

            var instructor = await _context.Instructors
                .Include(i => i.Department)
                .FirstOrDefaultAsync(i => i.UserId == instructorId);

            if (instructor == null)
            {
                return Unauthorized("Unauthorized access.");
            }

            // Fetch all majors, not restricted to the department
            var majors = await _context.Majors
                .Select(m => new SelectListItem
                {
                    Value = m.MajorId.ToString(),
                    Text = m.Name
                }).ToListAsync();

            var model = new CourseViewModel
            {
                AvailableMajors = majors,
                Title = "",
                Description = ""
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse(CourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var majors = await _context.Majors
                    .Select(m => new SelectListItem
                    {
                        Value = m.MajorId.ToString(),
                        Text = m.Name
                    }).ToListAsync();

                model.AvailableMajors = majors;
                return View(model);
            }

            var instructorDetails = await _context.Instructors
                .Include(i => i.Department)
                .FirstOrDefaultAsync(i => i.UserId == GetCurrentUserId());

            if (instructorDetails == null)
            {
                return Unauthorized("Unauthorized access.");
            }

            var allowedMajors = model.SelectedMajors != null && model.SelectedMajors.Any()
                ? string.Join(",", model.SelectedMajors)
                : string.Empty;

            var course = new CourseModel
            {
                Title = model.Title,
                Description = model.Description,
                MaxCapacity = model.MaxCapacity,
                CourseEndTime = model.CourseEndTime,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DepartmentId = instructorDetails.DepartmentId ?? 0,
                InstructorId = GetCurrentUserId(),
                AllowedMajors = allowedMajors // Save the selected majors as a string
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return RedirectToAction("CourseConfig", new { id = course.CourseId });
        }


        // 2. Edit Course - GET
        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == id && c.InstructorId == GetCurrentUserId());

            if (course == null)
            {
                return NotFound("Course not found or unauthorized access.");
            }

            var selectedMajors = course.AllowedMajors.Split(',')
                .Where(m => !string.IsNullOrEmpty(m))
                .Select(int.Parse)
                .ToList();

            var model = new CourseViewModel
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                MaxCapacity = course.MaxCapacity,
                CourseEndTime = course.CourseEndTime,
                //SelectedMajors = selectedMajors,
                AvailableMajors = await _context.Majors
                    .Select(m => new SelectListItem
                    {
                        Value = m.MajorId.ToString(),
                        Text = m.Name
                    }).ToListAsync()
            };

            return View(model);
        }

        // 2. Edit Course - POST
        [HttpPost]
        public async Task<IActionResult> EditCourse(CourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Re-fetch available majors if model state is invalid
                model.AvailableMajors = await _context.Majors
                    .Select(m => new SelectListItem
                    {
                        Value = m.MajorId.ToString(),
                        Text = m.Name
                    }).ToListAsync();
                return View(model);
            }

            var existingCourse = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == model.CourseId && c.InstructorId == GetCurrentUserId());

            if (existingCourse == null)
            {
                return Unauthorized("Unauthorized access or course not found.");
            }

            existingCourse.Title = model.Title;
            existingCourse.Description = model.Description;
            existingCourse.MaxCapacity = model.MaxCapacity;
            existingCourse.CourseEndTime = model.CourseEndTime;
            existingCourse.UpdatedAt = DateTime.UtcNow;
            existingCourse.AllowedMajors = model.SelectedMajors != null && model.SelectedMajors.Any()
                ? string.Join(",", model.SelectedMajors)
                : string.Empty;

            _context.Courses.Update(existingCourse);
            await _context.SaveChangesAsync();

            return RedirectToAction("CourseConfig", new { id = existingCourse.CourseId });
        }

        // 3. Delete Course - POST
        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Enrollments) // Include related enrollments
                .FirstOrDefaultAsync(c => c.CourseId == id && c.InstructorId == GetCurrentUserId());

            if (course == null)
            {
                return NotFound("Course not found or unauthorized access.");
            }

            // Remove related enrollments first
            if (course.Enrollments.Any())
            {
                _context.Enrollments.RemoveRange(course.Enrollments);
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return RedirectToAction("CoursesDashboard");
        }

        // 4. View Enrolled Students - GET
        [HttpGet]
        public async Task<IActionResult> ViewStudents(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.CourseId == id && c.InstructorId == GetCurrentUserId());

            if (course == null)
            {
                return NotFound("Course not found or unauthorized access.");
            }

            return View(course.Enrollments);
        }

        // 5. Courses Dashboard - GET
        [HttpGet]
        public async Task<IActionResult> CoursesDashboard()
        {
            var userId = GetCurrentUserId();
            var courses = await _context.Courses
                .Where(c => c.InstructorId == userId)
                .ToListAsync();

            return View(courses);
        }

        // 6. Course Configuration - GET
        public IActionResult CourseConfig(int id)
        {
            var course = _context.Courses
                .Include(c => c.Materials)
                .Include(c => c.Announcements)
                .Include(c => c.Quizzes)
                .FirstOrDefault(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            // Create a new CourseViewModel instance
            var courseViewModel = new CourseViewModel
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                MaxCapacity = course.MaxCapacity,
                CourseEndTime = course.CourseEndTime,
                SortedContent = new List<CourseContentViewModel>()
            };

            // Add materials to the view model
            courseViewModel.SortedContent.AddRange(course.Materials.Select(m => new CourseContentViewModel
            {
                Type = "Material",
                Material = m,
                CreatedAt = m.CreatedAt
            }));

            // Add announcements to the view model
            courseViewModel.SortedContent.AddRange(course.Announcements.Select(a => new CourseContentViewModel
            {
                Type = "Announcement",
                Announcement = a,
                CreatedAt = a.CreatedAt
            }));

            // Add quizzes to the view model
            courseViewModel.SortedContent.AddRange(course.Quizzes.Select(q => new CourseContentViewModel
            {
                Type = "Quiz",
                Quiz = q,
                CreatedAt = q.CreatedAt
            }));

            // Sort the combined content by CreatedAt property in descending order
            courseViewModel.SortedContent = courseViewModel.SortedContent.OrderByDescending(c => c.CreatedAt).ToList();

            return View(courseViewModel);
        }


    }
}
