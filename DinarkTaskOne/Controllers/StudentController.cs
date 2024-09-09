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
    public class StudentController(IEnrollmentService enrollmentService, ApplicationDbContext context) : Controller
    {
        // Action to display available quizzes for a course
        [HttpGet]
        public async Task<IActionResult> AvailableQuizzes(int courseId)
        {
            var quizzes = await context.Quizzes
                .Where(q => q.CourseId == courseId)
                .ToListAsync();

            return View(quizzes);
        }

        // Action to start a quiz
        [HttpGet]
        public async Task<IActionResult> TakeQuiz(int quizId)
        {
            // Retrieve the user's ID from the claims
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // If the userId is not valid, return Unauthorized
            if (!int.TryParse(userIdClaim, out int studentId))
            {
                return Unauthorized();
            }

            // Check if the student has already attempted and completed the quiz
            var existingAttempt = await context.Attempts
                .FirstOrDefaultAsync(a => a.QuizId == quizId && a.StudentId == studentId && a.Completed);

            if (existingAttempt != null)
            {
                // Redirect to the result if the quiz was already completed
                return RedirectToAction("QuizResult", new { attemptId = existingAttempt.AttemptId });
            }

            // Retrieve the quiz along with its questions and answers
            var quiz = await context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.QuizId == quizId);

            // If the quiz is not found, return a 404 error
            if (quiz == null)
            {
                return NotFound("Quiz not found.");
            }

            // Map the quiz data to the TakeQuizViewModel
            var takeQuizViewModel = new TakeQuizViewModel
            {
                QuizId = quiz.QuizId,
                QuizTitle = quiz.Title,
                Duration = quiz.Duration, // <-- Duration is passed to the view
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

            // Start a new quiz attempt for the student
            var attempt = new AttemptModel
            {
                QuizId = quiz.QuizId,
                StudentId = studentId,
                AttemptDate = DateTime.UtcNow,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.Add(quiz.Duration), // <-- Timer is based on the quiz duration
                Completed = false
            };

            // Save the attempt to the database
            context.Attempts.Add(attempt);
            await context.SaveChangesAsync();

            // Pass the attempt ID to the view via ViewBag
            ViewBag.AttemptId = attempt.AttemptId;

            // Return the TakeQuiz view with the view model
            return View(takeQuizViewModel);
        }

        // Action to submit the quiz
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

            // Check if the time limit has been exceeded
            if (DateTime.UtcNow > attempt.EndTime)
            {
                attempt.Score = 0;
                attempt.Completed = true;
                await context.SaveChangesAsync();
                ModelState.AddModelError("", "Time limit exceeded.");
                return RedirectToAction("QuizResult", new { attemptId = AttemptId });
            }

            // Check if the attempt is already completed
            if (attempt.Completed)
            {
                return BadRequest("This quiz has already been submitted.");
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int studentId) || studentId != attempt.StudentId)
            {
                return Unauthorized();
            }

            int totalScore = 0;
            int maxScore = attempt.Quiz.Questions.Sum(q => q.Marks);

            // Process each question
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

                        var questionAnswer = new QuestionAnswerModel
                        {
                            AttemptId = AttemptId,
                            QuestionId = question.QuestionId,
                            SelectedOptionId = selectedAnswerId // Ensure this is the selected option
                        };
                        context.QuestionAnswers.Add(questionAnswer);
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
                            var questionAnswer = new QuestionAnswerModel
                            {
                                AttemptId = AttemptId,
                                QuestionId = question.QuestionId,
                                SelectedOptionId = selectedAnswerId
                            };
                            context.QuestionAnswers.Add(questionAnswer);
                        }
                    }
                }
            }

            attempt.Score = totalScore;
            attempt.Completed = true;

            await context.SaveChangesAsync();

            return RedirectToAction("QuizResult", new { attemptId = AttemptId });
        }

        // Action to view quiz result
        [HttpGet]
        public async Task<IActionResult> QuizResult(int attemptId)
        {
            var attempt = await context.Attempts
                .Include(a => a.Quiz)
                .Include(a => a.QuestionAnswers) // Ensure QuestionAnswers is loaded
                    .ThenInclude(qa => qa.Question) // Load the related questions
                        .ThenInclude(q => q.Answers) // Load the related answers
                .FirstOrDefaultAsync(a => a.AttemptId == attemptId);

            if (attempt == null)
            {
                return NotFound("Attempt not found.");
            }

            ViewBag.Score = attempt.Score;
            ViewBag.MaxScore = attempt.Quiz.Questions.Sum(q => q.Marks);

            return View(attempt); // Make sure you're passing the populated model to the view
        }

        // Action to list courses and enrollments
        public async Task<IActionResult> MyCourses()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int studentId))
            {
                var enrollments = await enrollmentService.GetEnrollmentsByStudentIdAsync(studentId);
                return View(enrollments);
            }
            return Unauthorized();
        }

        // Enroll in a course
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

        // View course details
        [HttpGet]
        public async Task<IActionResult> ViewCourse(int courseId)
        {
            var course = await context.Courses
                .Include(c => c.Materials)
                .Include(c => c.Announcements)
                .Include(c => c.Quizzes) // Include quizzes for the course
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            var viewModel = new CourseViewModel
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                AnnouncementModelData = course.Announcements?.ToList() ?? [],
                MaterialsModelData = course.Materials?.ToList() ?? [],
                Quizzes = course.Quizzes?.ToList() ?? [] // Include available quizzes in the view
            };

            return View(viewModel);
        }
    }
}
