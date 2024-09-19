using DinarkTaskOne.Data;
using DinarkTaskOne.Models.MakeQuiz;
using DinarkTaskOne.Models.ManageCourse;
using DinarkTaskOne.Models.ViewModels;
using DinarkTaskOne.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DinarkTaskOne.Controllers
{
    public class StudentController : Controller
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly ApplicationDbContext _context;

        public StudentController(IEnrollmentService enrollmentService, ApplicationDbContext context)
        {
            _enrollmentService = enrollmentService;
            _context = context;
        }

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
            var quizzes = await _context.Quizzes
                .Where(q => q.CourseId == courseId)
                .ToListAsync();

            return View(quizzes);
        }

        // Start a quiz
        [HttpGet]
        public async Task<IActionResult> TakeQuiz(int quizId)
        {
            int studentId = GetCurrentUserId();

            var existingAttempt = await _context.Attempts
                .FirstOrDefaultAsync(a => a.QuizId == quizId && a.StudentId == studentId && a.Completed);

            if (existingAttempt != null)
            {
                return RedirectToAction("QuizResult", new { attemptId = existingAttempt.AttemptId });
            }

            var quiz = await _context.Quizzes
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

            _context.Attempts.Add(attempt);
            await _context.SaveChangesAsync();

            ViewBag.AttemptId = attempt.AttemptId;

            return View(takeQuizViewModel);
        }

        // Submit the quiz
        [HttpPost]
        public async Task<IActionResult> SubmitQuiz(int AttemptId, IFormCollection form)
        {
            var attempt = await _context.Attempts
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(a => a.AttemptId == AttemptId);

            if (attempt == null)
                return NotFound("Attempt not found.");

            if (DateTime.UtcNow > attempt.EndTime)
            {
                attempt.Score = 0;
                attempt.Completed = true;
                await _context.SaveChangesAsync();
                ModelState.AddModelError("", "Time limit exceeded.");
                return RedirectToAction("QuizResult", new { attemptId = AttemptId });
            }

            if (attempt.Completed)
            {
                return BadRequest("This quiz has already been submitted.");
            }

            int studentId = GetCurrentUserId();

            if (studentId != attempt.StudentId)
            {
                return Unauthorized();
            }

            int totalScore = 0;
            int maxScore = attempt.Quiz.Questions.Sum(q => q.Marks);

            foreach (var question in attempt.Quiz.Questions)
            {
                var formKey = $"SelectedAnswers[{question.QuestionId}]";
                var selectedValues = form[formKey];

                if (selectedValues.Count > 0)
                {
                    if (question.Type == QuestionType.MultipleChoice || question.Type == QuestionType.TrueFalse)
                    {
                        int selectedAnswerId = int.Parse(selectedValues[0]);
                        var selectedAnswer = question.Answers.FirstOrDefault(a => a.AnswerId == selectedAnswerId);

                        if (selectedAnswer != null && selectedAnswer.IsCorrect)
                        {
                            totalScore += question.Marks;
                        }

                        _context.QuestionAnswers.Add(new QuestionAnswerModel
                        {
                            AttemptId = AttemptId,
                            QuestionId = question.QuestionId,
                            SelectedOptionId = selectedAnswerId
                        });
                    }
                    else if (question.Type == QuestionType.MultipleAnswers)
                    {
                        var selectedAnswerIds = selectedValues.Select(int.Parse).ToList();
                        var correctAnswerIds = question.Answers
                            .Where(a => a.IsCorrect)
                            .Select(a => a.AnswerId)
                            .ToList();

                        if (!correctAnswerIds.Except(selectedAnswerIds).Any() && !selectedAnswerIds.Except(correctAnswerIds).Any())
                        {
                            totalScore += question.Marks;
                        }

                        foreach (var selectedAnswerId in selectedAnswerIds)
                        {
                            _context.QuestionAnswers.Add(new QuestionAnswerModel
                            {
                                AttemptId = AttemptId,
                                QuestionId = question.QuestionId,
                                SelectedOptionId = selectedAnswerId
                            });
                        }
                    }
                }
            }

            attempt.Score = totalScore;
            attempt.Completed = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("QuizResult", new { attemptId = AttemptId });
        }

        // View quiz result
        [HttpGet]
        public async Task<IActionResult> QuizResult(int attemptId)
        {
            var attempt = await _context.Attempts
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
            var enrollments = await _enrollmentService.GetEnrollmentsByStudentIdAsync(studentId);
            return View(enrollments);
        }

        // Enroll in a course
        [HttpGet]
        public async Task<IActionResult> Enroll()
        {
            ViewBag.Courses = await _context.Courses
                .Include(c => c.Instructor)
                .ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EnrollPost(int courseId)
        {
            if (courseId <= 0)
            {
                ModelState.AddModelError("", "Please select a valid course.");
                ViewBag.Courses = await _context.Courses.Include(c => c.Instructor).ToListAsync();
                return View("Enroll");
            }

            var course = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
            {
                ModelState.AddModelError("", "The selected course does not exist.");
                ViewBag.Courses = await _context.Courses.Include(c => c.Instructor).ToListAsync();
                return View("Enroll");
            }

            if (course.Enrollments.Count >= course.MaxCapacity)
            {
                ModelState.AddModelError("", "This course has reached its maximum capacity.");
                ViewBag.Courses = await _context.Courses.Include(c => c.Instructor).ToListAsync();
                return View("Enroll");
            }

            int studentId = GetCurrentUserId();
            if (await _enrollmentService.IsStudentEnrolledAsync(studentId, courseId))
            {
                ModelState.AddModelError("", "You are already enrolled in this course.");
                ViewBag.Courses = await _context.Courses.Include(c => c.Instructor).ToListAsync();
                return View("Enroll");
            }

            return View("EnrollPost", course);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmEnrollment(int courseId)
        {
            int studentId = GetCurrentUserId();
            var enrollmentSuccessful = await _enrollmentService.EnrollStudentAsync(studentId, courseId);
            if (enrollmentSuccessful)
            {
                ViewBag.Message = "Enrollment successful!";
                return RedirectToAction("MyCourses");
            }
            ModelState.AddModelError("", "An error occurred while trying to enroll.");
            return RedirectToAction("Enroll", new { courseId });
        }

        // View course details
        [HttpGet]
        public async Task<IActionResult> ViewCourse(int courseId)
        {
            var studentId = GetCurrentUserId();

            var course = await _context.Courses
                .Include(c => c.Materials)
                .Include(c => c.Announcements)
                .Include(c => c.Quizzes)
                .ThenInclude(q => q.Questions)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            var completedAttempts = await _context.Attempts
                .Where(a => a.Quiz.CourseId == courseId && a.StudentId == studentId && a.Completed)
                .ToListAsync();

            var completedQuizzes = completedAttempts.Select(a => a.QuizId).ToList();
            var completedAttemptsDict = completedAttempts.ToDictionary(a => a.QuizId, a => a.AttemptId);

            var combinedQuizScore = completedAttempts.Sum(a => a.Score.Value);
            int totalMaxScore = course.Quizzes.Sum(q => q.Questions.Sum(qn => qn.Marks));

            var percentageScore = totalMaxScore > 0
                ? (combinedQuizScore / (double)totalMaxScore) * 100
                : 0;

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

            sortedContent = sortedContent.OrderByDescending(c => c.CreatedAt).ToList();

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
                CompletedAttempts = completedAttemptsDict
            };

            return View(viewModel);
        }

    }

}
