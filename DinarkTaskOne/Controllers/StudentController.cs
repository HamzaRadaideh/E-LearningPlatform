using DinarkTaskOne.Data;
using DinarkTaskOne.Models.MakeQuiz;
using DinarkTaskOne.Models.ManageCourse;
using DinarkTaskOne.Models.student;
using DinarkTaskOne.Models.UserSpecficModels;
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
        // Helper method to get the current user's ID
        private int GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("User ID claim is missing.");
            return int.Parse(userIdValue);
        }

        // Display available quizzes for a course
        [HttpGet]
        public async Task<IActionResult> AvailableQuizzes(int courseId)
        {
            var quizzes = await context.Quizzes
                .Where(q => q.CourseId == courseId)
                .ToListAsync();

            return View(quizzes);
        }

        // Start a quiz
        [HttpGet]
        public async Task<IActionResult> TakeQuiz(int quizId)
        {
            int studentId = GetCurrentUserId();

            var existingAttempt = await context.Attempts
                .FirstOrDefaultAsync(a => a.QuizId == quizId && a.StudentId == studentId && a.Completed);

            if (existingAttempt != null)
            {
                return RedirectToAction("QuizResult", new { attemptId = existingAttempt.AttemptId });
            }

            var quiz = await context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.QuizId == quizId);

            if (quiz == null)
            {
                return NotFound("Quiz not found.");
            }

            var takeQuizViewModel = new TakeQuizViewModel
            {
                QuizId = quiz.QuizId,
                QuizTitle = quiz.Title,
                Duration = quiz.Duration,
                Questions = quiz.Questions.Select(q => new QuestionViewModel
                {
                    QuestionId = q.QuestionId,
                    Text = q.Text,
                    Type = q.Type,
                    Answers = q.Answers.Select(a => new AnswerViewModel
                    {
                        AnswerId = a.AnswerId,
                        Text = a.Text
                    }).ToList()
                }).ToList()
            };

            var attempt = new AttemptModel
            {
                QuizId = quiz.QuizId,
                StudentId = studentId,
                AttemptDate = DateTime.UtcNow,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.Add(quiz.Duration),
                Completed = false
            };

            context.Attempts.Add(attempt);
            await context.SaveChangesAsync();

            ViewBag.AttemptId = attempt.AttemptId;

            return View(takeQuizViewModel);
        }

        // Submit the quiz
        [HttpPost]
        public async Task<IActionResult> SubmitQuiz(int AttemptId, IFormCollection form)
        {
            var attempt = await context.Attempts
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(a => a.AttemptId == AttemptId);

            if (attempt == null)
                return NotFound("Attempt not found.");

            if (DateTime.UtcNow > attempt.EndTime || attempt.Completed)
            {
                attempt.Score = 0;
                attempt.Completed = true;
                await context.SaveChangesAsync();
                ModelState.AddModelError("", "Time limit exceeded or quiz already submitted.");
                return RedirectToAction("QuizResult", new { attemptId = AttemptId });
            }

            int studentId = GetCurrentUserId();
            if (studentId != attempt.StudentId)
                return Unauthorized();

            double totalScore = 0;
            double maxScore = attempt.Quiz.Questions.Sum(q => q.Marks);

            foreach (var question in attempt.Quiz.Questions)
            {
                var formKey = $"SelectedAnswers[{question.QuestionId}]";
                var selectedValues = form[formKey];
                var selectedAnswerIds = selectedValues.ToList().Select(int.Parse).ToList();

                if (selectedValues.Count > 0 && !string.IsNullOrWhiteSpace(selectedValues[0]))
                {
                    var correctAnswers = question.Answers.Where(a => a.IsCorrect).Select(a => a.AnswerId).ToList();

                    if (question.Type == QuestionType.MultipleChoice || question.Type == QuestionType.TrueFalse)
                    {
                        var isCorrect = correctAnswers.SequenceEqual(selectedAnswerIds);
                        if (isCorrect)
                            totalScore += question.Marks;
                    }
                    else if (question.Type == QuestionType.MultipleAnswers)
                    {
                        var isCorrect = correctAnswers.All(c => selectedAnswerIds.Contains(c)) &&
                                        selectedAnswerIds.All(s => correctAnswers.Contains(s));
                        if (isCorrect)
                            totalScore += question.Marks;
                    }

                    foreach (var answerId in selectedAnswerIds)
                    {
                        context.QuestionAnswers.Add(new QuestionAnswerModel
                        {
                            AttemptId = AttemptId,
                            QuestionId = question.QuestionId,
                            SelectedOptionId = answerId
                        });
                    }
                }
            }

            attempt.Score = (int?)Math.Round(totalScore);
            attempt.Completed = true;
            await context.SaveChangesAsync();

            // Update student progress and grade
            await enrollmentService.RecordStudentProgressAsync(studentId, attempt.Quiz.CourseId, totalScore);

            return RedirectToAction("QuizResult", new { attemptId = AttemptId });
        }

        // View quiz result
        [HttpGet]
        public async Task<IActionResult> QuizResult(int attemptId)
        {
            var attempt = await context.Attempts
                .Include(a => a.Quiz)
                .Include(a => a.QuestionAnswers)
                    .ThenInclude(qa => qa.Question)
                        .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(a => a.AttemptId == attemptId);

            if (attempt == null)
            {
                return NotFound("Attempt not found.");
            }

            ViewBag.Score = attempt.Score;
            ViewBag.MaxScore = attempt.Quiz.Questions.Sum(q => q.Marks);

            return View(attempt);
        }

        // List courses and enrollments
        public async Task<IActionResult> MyCourses()
        {
            int studentId = GetCurrentUserId();
            var enrollments = await enrollmentService.GetEnrollmentsByStudentIdAsync(studentId);
            return View(enrollments);
        }

        // Enroll in a course - GET
        // Enroll in a course - GET
        [HttpGet]
        public async Task<IActionResult> Enroll()
        {
            int studentId = GetCurrentUserId();

            var student = await context.Students
                .Include(s => s.Major)
                .Include(s => s.CurrentLevel)
                .FirstOrDefaultAsync(s => s.UserId == studentId);

            if (student == null)
            {
                return NotFound("Student not found.");
            }

            var availableCourses = await context.Courses
                .Where(c => c.LevelId == student.CurrentLevelId &&
                            c.AllowedMajors.Contains(student.MajorId.ToString()) &&
                            c.Status == StatusType.Active)
                .Include(c => c.Instructor)
                .ToListAsync();

            // Check if any available courses exist
            if (availableCourses == null || !availableCourses.Any())
            {
                ViewBag.Message = "No courses available for enrollment.";
            }

            // Populate the ViewBag.Courses with available courses
            ViewBag.Courses = availableCourses;
            return View();
        }



        // Enroll in a course - POST
        // Enroll in a course - POST
        [HttpPost]
        public async Task<IActionResult> EnrollPost(int courseId)
        {
            int studentId = GetCurrentUserId();

            // Attempt to enroll the student
            if (await enrollmentService.EnrollStudentAsync(studentId, courseId))
            {
                ViewBag.Message = "Enrollment successful!";
                return RedirectToAction("MyCourses");
            }

            // If enrollment fails, repopulate ViewBag.Courses and display the form again
            var student = await context.Students
                .Include(s => s.Major)
                .Include(s => s.CurrentLevel)
                .FirstOrDefaultAsync(s => s.UserId == studentId);

            if (student == null)
            {
                return NotFound("Student not found.");
            }

            var availableCourses = await context.Courses
                .Where(c => c.LevelId == student.CurrentLevelId &&
                            c.AllowedMajors.Contains(student.MajorId.ToString()) &&
                            c.Status == StatusType.Active)
                .Include(c => c.Instructor)
                .ToListAsync();

            ViewBag.Courses = availableCourses; // Repopulate ViewBag.Courses

            ModelState.AddModelError("", "Failed to enroll. Please try again.");
            return View("Enroll");
        }



        // View course details
        [HttpGet]
        public async Task<IActionResult> ViewCourse(int courseId)
        {
            var studentId = GetCurrentUserId();

            // Fetch the course with its related entities
            var course = await context.Courses
                .Include(c => c.Materials)
                .Include(c => c.Announcements)
                .Include(c => c.Quizzes)
                .ThenInclude(q => q.Questions)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            // Fetch completed attempts for the student in this course
            var completedAttempts = await context.Attempts
                .Where(a => a.Quiz.CourseId == courseId && a.StudentId == studentId && a.Completed)
                .ToListAsync();

            // Calculate the total quiz score and max possible score
            int combinedQuizScore = completedAttempts.Sum(a => a.Score ?? 0);

            int totalMaxScore = course.Quizzes
                .SelectMany(q => q.Questions)
                .Sum(q => q.Marks);

            // Calculate percentage score
            var percentageScore = totalMaxScore > 0
                ? (combinedQuizScore / (double)totalMaxScore) * 100
                : 0;

            // Get the list of completed quizzes and their corresponding attempts
            var completedQuizzes = completedAttempts.Select(a => a.QuizId).ToList();
            var completedAttemptsDict = completedAttempts.ToDictionary(a => a.QuizId, a => a.AttemptId);

            // Prepare the sorted content list
            var sortedContent = new List<CourseContentViewModel>();

            sortedContent.AddRange(course.Materials.Select(m => new CourseContentViewModel
            {
                Type = "Material",
                Material = m,
                CreatedAt = m.CreatedAt
            }));

            sortedContent.AddRange(course.Announcements.Select(a => new CourseContentViewModel
            {
                Type = "Announcement",
                Announcement = a,
                CreatedAt = a.CreatedAt
            }));

            sortedContent.AddRange(course.Quizzes.Select(q => new CourseContentViewModel
            {
                Type = "Quiz",
                Quiz = q,
                CreatedAt = q.CreatedAt
            }));

            // Order the content by creation date
            sortedContent = sortedContent.OrderByDescending(c => c.CreatedAt).ToList();

            // Fetch the student's grade model
            var studentGradeModel = await context.StudentGrades
                .FirstOrDefaultAsync(sg => sg.StudentId == studentId && sg.LevelId == course.LevelId);

            // Assign student grade and pass/fail status
            double? studentGrade = null;
            bool hasPassed = false;
            string? overallGrade = null;

            if (studentGradeModel != null)
            {
                studentGrade = studentGradeModel.AverageScore;
                hasPassed = studentGradeModel.HasPassed;
                overallGrade = studentGradeModel.OverallGrade;
            }

            // Construct the view model with all the necessary data
            var viewModel = new CourseViewModel
            {
                CourseId = courseId,
                Title = course.Title,
                Description = course.Description,
                SortedContent = sortedContent,
                CombinedQuizScore = combinedQuizScore,
                MaxPossibleScore = totalMaxScore,
                PercentageScore = percentageScore,
                CompletedQuizzes = completedQuizzes,
                CompletedAttempts = completedAttemptsDict,
                Status = course.Status, // Add course status
                StudentGrade = studentGrade, // Add student grade if available
                OverallGrade = overallGrade, // Add overall grade letter if available
                HasPassed = hasPassed // Add pass/fail status
            };

            // Return the view with the model
            return View(viewModel);
        }


    }
}
