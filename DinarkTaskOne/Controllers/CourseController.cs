using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using DinarkTaskOne.Data;
using DinarkTaskOne.Models.ManageCourse;
using DinarkTaskOne.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using DinarkTaskOne.Models.student;

namespace DinarkTaskOne.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class CourseController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext context = context;

        // Helper method to get the current user's ID
        private int GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("User ID claim is missing.");
            return int.Parse(userIdValue);
        }

        // Helper method to check if a course belongs to the current instructor
        private async Task<bool> IsInstructorAuthorizedAsync(int courseId)
        {
            var course = await context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            return course != null && course.InstructorId == GetCurrentUserId();
        }

        // 1. Create Course - GET
        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
            var instructorId = GetCurrentUserId();

            var instructor = await context.Instructors
                .Include(i => i.Department)
                .FirstOrDefaultAsync(i => i.UserId == instructorId);

            if (instructor == null)
            {
                return Unauthorized("Unauthorized access.");
            }

            var majors = await context.Majors
                .Select(m => new SelectListItem
                {
                    Value = m.MajorId.ToString(),
                    Text = m.Name
                }).ToListAsync();

            var levels = await context.Levels
                .Select(l => new SelectListItem
                {
                    Value = l.LevelId.ToString(),
                    Text = l.Type.ToString()
                }).ToListAsync();

            var model = new CourseViewModel
            {
                AvailableMajors = majors,
                AvailableLevels = levels,
                Title = "",
                Description = ""
            };

            return View(model);
        }

        // 1. Create Course - POST
        [HttpPost]
        public async Task<IActionResult> CreateCourse(CourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var instructorId = GetCurrentUserId();

                var majors = await context.Majors
                    .Where(m => m.Department.Instructors.Any(i => i.UserId == instructorId))
                    .Select(m => new SelectListItem
                    {
                        Value = m.MajorId.ToString(),
                        Text = m.Name
                    }).ToListAsync();

                var levels = await context.Levels
                    .Select(l => new SelectListItem
                    {
                        Value = l.LevelId.ToString(),
                        Text = l.Type.ToString()
                    }).ToListAsync();

                model.AvailableMajors = majors;
                model.AvailableLevels = levels;
                return View(model);
            }

            var instructorDetails = await context.Instructors
                .Include(i => i.Department)
                .FirstOrDefaultAsync(i => i.UserId == GetCurrentUserId());

            if (instructorDetails == null)
            {
                return Unauthorized("Unauthorized access.");
            }

            var allowedMajors = model.SelectedMajors != null && model.SelectedMajors.Count != 0
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
                DepartmentId = instructorDetails.DepartmentId,
                InstructorId = GetCurrentUserId(),
                AllowedMajors = allowedMajors,
                LevelId = model.SelectedLevelId,
                Status = StatusType.Active
            };

            context.Courses.Add(course);
            await context.SaveChangesAsync();

            return RedirectToAction("CourseConfig", new { id = course.CourseId });
        }

        // 2. Edit Course - GET
        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            if (!await IsInstructorAuthorizedAsync(id))
            {
                return Unauthorized("Unauthorized access.");
            }

            var course = await context.Courses.FirstOrDefaultAsync(c => c.CourseId == id);
            if (course == null)
            {
                return NotFound("Course not found.");
            }

            var selectedMajors = course.AllowedMajors.Split(',')
                .Where(m => !string.IsNullOrEmpty(m))
                .Select(int.Parse)
                .ToList();

            var availableMajors = await context.Majors
                .Where(m => m.Department.Courses.Any(c => c.CourseId == id))
                .Select(m => new SelectListItem
                {
                    Value = m.MajorId.ToString(),
                    Text = m.Name
                }).ToListAsync();

            var availableLevels = await context.Levels
                .Select(l => new SelectListItem
                {
                    Value = l.LevelId.ToString(),
                    Text = l.Type.ToString()
                }).ToListAsync();

            var model = new CourseViewModel
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                MaxCapacity = course.MaxCapacity,
                CourseEndTime = course.CourseEndTime,
                AvailableMajors = availableMajors,
                AvailableLevels = availableLevels,
                SelectedLevelId = course.LevelId,
                SelectedMajors = selectedMajors
            };

            return View(model);
        }

        // 2. Edit Course - POST
        [HttpPost]
        public async Task<IActionResult> EditCourse(CourseViewModel model)
        {
            if (!await IsInstructorAuthorizedAsync(model.CourseId))
            {
                return Unauthorized("Unauthorized access.");
            }

            if (!ModelState.IsValid)
            {
                model.AvailableMajors = await context.Majors
                    .Where(m => m.Department.Courses.Any(c => c.CourseId == model.CourseId))
                    .Select(m => new SelectListItem
                    {
                        Value = m.MajorId.ToString(),
                        Text = m.Name
                    }).ToListAsync();

                model.AvailableLevels = await context.Levels
                    .Select(l => new SelectListItem
                    {
                        Value = l.LevelId.ToString(),
                        Text = l.Type.ToString()
                    }).ToListAsync();

                return View(model);
            }

            var existingCourse = await context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == model.CourseId);

            if (existingCourse == null)
            {
                return NotFound("Course not found.");
            }

            existingCourse.Title = model.Title;
            existingCourse.Description = model.Description;
            existingCourse.MaxCapacity = model.MaxCapacity;
            existingCourse.CourseEndTime = model.CourseEndTime;
            existingCourse.UpdatedAt = DateTime.UtcNow;
            existingCourse.AllowedMajors = model.SelectedMajors != null && model.SelectedMajors.Count != 0
                ? string.Join(",", model.SelectedMajors)
                : string.Empty;
            existingCourse.LevelId = model.SelectedLevelId;

            context.Courses.Update(existingCourse);
            await context.SaveChangesAsync();

            return RedirectToAction("CourseConfig", new { id = existingCourse.CourseId });
        }

        // 3. Delete Course - POST
        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            if (!await IsInstructorAuthorizedAsync(id))
            {
                return Unauthorized("Unauthorized access.");
            }

            // Retrieve the course and include all related entities
            var course = await context.Courses
                .Include(c => c.Enrollments)
                .Include(c => c.Quizzes)
                    .ThenInclude(q => q.Questions)
                .Include(c => c.Quizzes)
                    .ThenInclude(q => q.Attempts)
                .Include(c => c.Materials)
                .Include(c => c.Announcements)
                .Include(c => c.CourseGrades)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            // Remove related CourseGrades
            if (course.CourseGrades.Any())
            {
                context.CourseGrades.RemoveRange(course.CourseGrades);
            }

            // Remove related Enrollments
            if (course.Enrollments.Any())
            {
                context.Enrollments.RemoveRange(course.Enrollments);
            }

            // Remove related Quizzes along with their Questions and Attempts
            if (course.Quizzes.Any())
            {
                foreach (var quiz in course.Quizzes)
                {
                    if (quiz.Questions.Any())
                    {
                        context.Questions.RemoveRange(quiz.Questions);
                    }

                    if (quiz.Attempts.Any())
                    {
                        context.Attempts.RemoveRange(quiz.Attempts);
                    }
                }

                context.Quizzes.RemoveRange(course.Quizzes);
            }

            // Remove related Materials
            if (course.Materials.Any())
            {
                context.Materials.RemoveRange(course.Materials);
            }

            // Remove related Announcements
            if (course.Announcements.Any())
            {
                context.Announcements.RemoveRange(course.Announcements);
            }

            // Finally, remove the course itself
            context.Courses.Remove(course);

            // Save all changes in one go
            await context.SaveChangesAsync();

            return RedirectToAction("CoursesDashboard");
        }

        // 4. View Enrolled Students - GET
        [HttpGet]
        public async Task<IActionResult> ViewStudents(int id)
        {
            if (!await IsInstructorAuthorizedAsync(id))
            {
                return Unauthorized("Unauthorized access.");
            }

            var course = await context.Courses
                .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            return View(course.Enrollments);
        }

        // 5. Courses Dashboard - GET
        [HttpGet]
        public async Task<IActionResult> CoursesDashboard()
        {
            var userId = GetCurrentUserId();
            var courses = await context.Courses
                .Where(c => c.InstructorId == userId)
                .ToListAsync();
            return View(courses);
        }

        // 6. Course Configuration - GET
        public async Task<IActionResult> CourseConfig(int id)
        {
            if (!await IsInstructorAuthorizedAsync(id))
            {
                return Unauthorized("Unauthorized access.");
            }

            var course = await context.Courses
                .Include(c => c.Materials)
                .Include(c => c.Announcements)
                .Include(c => c.Quizzes)
                .ThenInclude(q => q.Questions) // Include questions for quizzes
                .Include(c => c.Enrollments)
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            // Calculate remaining marks
            var remainingMarks = CalculateRemainingMarks(course);

            var courseViewModel = new CourseViewModel
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                MaxCapacity = course.MaxCapacity,
                CourseEndTime = course.CourseEndTime,
                Status = course.Status,
                Enrollments = course.Enrollments.ToList(),
                RemainingMarks = remainingMarks, // Assign the calculated remaining marks
                CourseCompletionPercentage = CalculateCourseCompletionPercentage(course),
                QuizCompletionPercentage = CalculateQuizCompletionPercentage(course),
                SortedContent = course.Materials
                    .Select(m => new CourseContentViewModel { Type = "Material", Material = m, CreatedAt = m.CreatedAt })
                    .Union(course.Announcements.Select(a => new CourseContentViewModel { Type = "Announcement", Announcement = a, CreatedAt = a.CreatedAt }))
                    .Union(course.Quizzes.Select(q => new CourseContentViewModel { Type = "Quiz", Quiz = q, CreatedAt = q.CreatedAt }))
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList()
            };

            return View(courseViewModel);
        }



        // 7. End Course and Calculate Grades - POST
        [HttpPost]
        public async Task<IActionResult> EndCourse(int courseId)
        {
            if (!await IsInstructorAuthorizedAsync(courseId))
            {
                return Unauthorized("Unauthorized access.");
            }

            var course = await context.Courses
                .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            if (course.Status == StatusType.Completed)
            {
                return BadRequest("Course is already completed.");
            }

            // Update course status to Completed
            course.Status = StatusType.Completed;

            // Calculate final grades for all enrolled students
            foreach (var enrollment in course.Enrollments)
            {
                var finalGrade = await CalculateCourseGradeAsync(enrollment.StudentId, course.CourseId);
                await CalculateOverallGradeAsync(enrollment.StudentId, course.LevelId);
            }

            // Save the changes to the course
            context.Courses.Update(course);
            await context.SaveChangesAsync();

            return RedirectToAction("CourseDetails", new { id = course.CourseId });
        }

        // Calculate grade for a specific course and update the student's progress
        private async Task<CourseGradeModel?> CalculateCourseGradeAsync(int studentId, int courseId)
        {
            var course = await context.Courses
                .Include(c => c.Quizzes)
                .ThenInclude(q => q.Questions)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
                return null;

            var completedAttempts = await context.Attempts
                .Where(a => a.StudentId == studentId && a.Quiz.CourseId == courseId && a.Completed)
                .ToListAsync();

            var combinedQuizScore = completedAttempts.Sum(a => a.Score ?? 0);

            var maxScore = course.Quizzes
                .SelectMany(q => q.Questions)
                .Sum(qn => qn.Marks);

            var score = maxScore > 0 ? (combinedQuizScore / (double)maxScore) * 100 : 0;

            var courseGrade = await context.CourseGrades
                .FirstOrDefaultAsync(cg => cg.StudentId == studentId && cg.CourseId == courseId);

            if (courseGrade == null)
            {
                courseGrade = new CourseGradeModel
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    Score = score,
                    HasPassed = score >= 50
                };
                context.CourseGrades.Add(courseGrade);
            }
            else
            {
                courseGrade.Score = score;
                courseGrade.HasPassed = score >= 50;
                context.CourseGrades.Update(courseGrade);
            }

            await context.SaveChangesAsync();
            return courseGrade;
        }

        // Calculate overall grade for a student in a specific level
        private async Task<StudentGradeModel?> CalculateOverallGradeAsync(int studentId, int levelId)
        {
            var progress = await context.StudentProgresses
                .Where(sp => sp.StudentId == studentId && sp.LevelId == levelId)
                .ToListAsync();

            if (progress.Count == 0)
                return null;

            double totalScore = progress.Sum(sp => sp.Score);
            double averageScore = progress.Count > 0 ? totalScore / progress.Count : 0;

            var studentGrade = await context.StudentGrades
                .FirstOrDefaultAsync(sg => sg.StudentId == studentId && sg.LevelId == levelId);

            if (studentGrade == null)
            {
                studentGrade = new StudentGradeModel
                {
                    StudentId = studentId,
                    LevelId = levelId,
                    AverageScore = averageScore,
                    HasPassed = averageScore >= 50,
                    OverallGrade = CalculateOverallGrade(averageScore),
                    CalculatedAt = DateTime.UtcNow
                };
                context.StudentGrades.Add(studentGrade);
            }
            else
            {
                studentGrade.AverageScore = averageScore;
                studentGrade.HasPassed = averageScore >= 50;
                studentGrade.OverallGrade = CalculateOverallGrade(averageScore);
                studentGrade.CalculatedAt = DateTime.UtcNow;
                context.StudentGrades.Update(studentGrade);
            }

            await context.SaveChangesAsync();
            return studentGrade;
        }

        // Calculate Overall Grade based on Average Score
        private static string CalculateOverallGrade(double averageScore)
        {
            if (averageScore >= 90)
                return "A";
            if (averageScore >= 80)
                return "B";
            if (averageScore >= 70)
                return "C";
            if (averageScore >= 60)
                return "D";
            return "F";
        }

        // Calculate the remaining marks for the course
        private double CalculateRemainingMarks(CourseModel course)
        {
            const double maxTotalMarks = 100;

            // Calculate the total marks from all quizzes
            var totalMarks = course.Quizzes
                .SelectMany(q => q.Questions)
                .Sum(q => q.Marks);

            // Calculate the remaining marks
            double remainingMarks = maxTotalMarks - totalMarks;
            Console.WriteLine($"Total Marks: {totalMarks}, Remaining Marks: {remainingMarks}");

            // Return the remaining marks, which can be negative if total marks exceed 100
            return remainingMarks;
        }


        //Calculate the course completion percentage based on content and quiz completion
        private double CalculateCourseCompletionPercentage(CourseModel course)
        {
            var materialsCount = course.Materials.Count;
            var announcementsCount = course.Announcements.Count;
            var quizzesCount = course.Quizzes.Count;

            var totalItems = materialsCount + announcementsCount + quizzesCount;
            if (totalItems == 0) return 0;

            var completedItems = materialsCount + announcementsCount + course.Quizzes.Count(q => q.Questions.Any());
            return (completedItems / (double)totalItems) * 100;
        }

        //Calculate the quiz completion percentage based on the number of completed quizzes
        private double CalculateQuizCompletionPercentage(CourseModel course)
        {
            var totalQuizzes = course.Quizzes.Count;
            if (totalQuizzes == 0) return 0;

            var completedQuizzes = course.Quizzes.Count(q => q.Questions.Any());
            return (completedQuizzes / (double)totalQuizzes) * 100;
        }


        [HttpPost]
        public async Task<IActionResult> DistributeBonusMarks(int courseId, double bonusMarks)
        {
            Console.WriteLine($"DistributeBonusMarks called with courseId: {courseId}, bonusMarks: {bonusMarks}");

            if (!await IsInstructorAuthorizedAsync(courseId))
            {
                return Unauthorized("Unauthorized access.");
            }

            var course = await context.Courses
                .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            var remainingMarks = CalculateRemainingMarks(course);

            if (bonusMarks <= 0 || bonusMarks > remainingMarks)
            {
                return BadRequest("Invalid bonus marks amount.");
            }

            foreach (var enrollment in course.Enrollments)
            {
                // Check if student progress already exists
                var studentProgress = await context.StudentProgresses
                    .FirstOrDefaultAsync(sp => sp.StudentId == enrollment.StudentId && sp.LevelId == course.LevelId);

                if (studentProgress != null)
                {
                    studentProgress.Score += bonusMarks;
                    context.StudentProgresses.Update(studentProgress);
                }
            }

            // Update remaining marks in the course model
            course.RemainingMarks = remainingMarks - bonusMarks; // Decrease the remaining marks
            context.Courses.Update(course); // Update the course model

            await context.SaveChangesAsync();
            return RedirectToAction("CourseConfig", new { id = courseId });
        }
    }
}
