﻿using DinarkTaskOne.Attributes;
using DinarkTaskOne.Data;
using DinarkTaskOne.Models.MakeQuiz;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using DinarkTaskOne.Models.ManageCourse;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DinarkTaskOne.Controllers
{
    //[Authorize(Roles = "Instructor")]
    public class QuizController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext context = context;

        // 1. Create a new quiz (Instructor only)
        [HttpGet]
        public async Task<IActionResult> CreateQuiz(int courseId)
        {
            var remainingMarks = await CalculateRemainingMarksAsync(courseId);

            ViewBag.CourseId = courseId;
            ViewBag.RemainingMarks = remainingMarks;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuiz(QuizModel quiz, int hours, int minutes, int seconds, int numberOfQuestions)
        {
            // Check if CourseId is valid and the course exists
            var course = await context.Courses.FirstOrDefaultAsync(c => c.CourseId == quiz.CourseId);
            if (course == null)
            {
                ModelState.AddModelError("", $"Course with ID {quiz.CourseId} does not exist.");
                ViewBag.CourseId = quiz.CourseId;
                return View(quiz);
            }

            // Initialize the quiz's Questions property to prevent null references
            quiz.Questions ??= [];

            // Calculate remaining marks for the course
            var remainingMarks = await CalculateRemainingMarksAsync(quiz.CourseId);

            // Check if the new quiz would exceed the remaining marks
            if (quiz.Questions.Sum(q => q.Marks) > remainingMarks)
            {
                ModelState.AddModelError("", $"Total marks for this quiz exceed the remaining marks of {remainingMarks} for the course.");
                ViewBag.CourseId = quiz.CourseId;
                ViewBag.RemainingMarks = remainingMarks; // Re-populate remaining marks
                return View(quiz);
            }

            // Combine the time inputs into a TimeSpan
            quiz.Duration = new TimeSpan(hours, minutes, seconds);
            quiz.NumberOfQuestions = numberOfQuestions;

            // Save the quiz to the database
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            // Initialize session to track current question
            HttpContext.Session.SetInt32("TotalQuestions", quiz.NumberOfQuestions);
            HttpContext.Session.SetInt32("CurrentQuestion", 1);

            // Redirect to AddQuestions to start adding questions to the quiz
            return RedirectToAction("AddQuestions", new { quizId = quiz.QuizId });
        }


        // 2. Add questions to the quiz, allowing the instructor to specify the type of question
        [HttpGet]
        public async Task<IActionResult> AddQuestions(int quizId)
        {
            int? totalQuestions = HttpContext.Session.GetInt32("TotalQuestions");
            int? currentQuestion = HttpContext.Session.GetInt32("CurrentQuestion");

            if (totalQuestions == null || currentQuestion == null)
                return RedirectToAction("CreateQuiz");

            var quiz = await context.Quizzes.FindAsync(quizId);
            if (quiz == null) return NotFound("Quiz not found.");

            if (currentQuestion > totalQuestions)
                return RedirectToAction("QuizComplete", new { quizId = quiz.QuizId });

            ViewBag.QuizId = quizId;
            ViewBag.CurrentQuestion = currentQuestion;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddQuestions(QuestionModel questionModel, string questionType)
        {
            if (Enum.TryParse<QuestionType>(questionType, out var parsedQuestionType))
            {
                questionModel.Type = parsedQuestionType;
            }
            else
            {
                return BadRequest("Invalid question type.");
            }

            // Validate the question's text and marks
            if (string.IsNullOrEmpty(questionModel.Text))
            {
                ModelState.AddModelError("", "The question text cannot be empty.");
                return View(questionModel);
            }

            if (questionModel.Marks <= 0)
            {
                ModelState.AddModelError("", "Marks must be greater than zero.");
                return View(questionModel);
            }

            // Save the question
            context.Questions.Add(questionModel);
            await context.SaveChangesAsync();

            // Redirect to AddOptions to add options to the question
            return RedirectToAction("AddOptions", new { questionId = questionModel.QuestionId, questionType });
        }

        // 3. Add options/answers to a question, depending on the question type
        [HttpGet]
        public async Task<IActionResult> AddOptions(int questionId, string questionType)
        {
            var question = await context.Questions.FindAsync(questionId);
            if (question == null) return NotFound("Question not found.");

            ViewBag.QuestionId = questionId;
            ViewBag.QuestionType = questionType;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitOptions(List<AnswerModel> answerModels, int questionId, string? questionType, int? correctAnswer)
        {
            if (string.IsNullOrEmpty(questionType))
            {
                ModelState.AddModelError("", "Question type is required.");
                return View("AddOptions", answerModels); // Returning back to AddOptions view
            }

            // Find the question
            var question = await context.Questions.FindAsync(questionId);
            if (question == null) return NotFound("Question not found.");

            // Validate the number of options based on the question type
            int expectedOptions = question.Type switch
            {
                QuestionType.TrueFalse => 2,
                QuestionType.MultipleChoice => 4,
                QuestionType.MultipleAnswers => 5,
                _ => 0
            };

            // Ensure the correct number of options
            if (answerModels.Count != expectedOptions)
            {
                ModelState.AddModelError("", $"The {question.Type} question requires exactly {expectedOptions} options.");
                return View("AddOptions", answerModels); // Return to AddOptions view in case of error
            }

            // Add the options to the question
            for (int i = 0; i < answerModels.Count; i++)
            {
                answerModels[i].QuestionId = questionId;

                // Handle True/False and Multiple Choice differently
                if (question.Type == QuestionType.MultipleChoice || question.Type == QuestionType.TrueFalse)
                {
                    // Set only the selected radio button option as correct
                    answerModels[i].IsCorrect = (i == correctAnswer);
                }
                else if (question.Type == QuestionType.MultipleAnswers)
                {
                    // Use the IsCorrect value for checkboxes to set multiple correct answers
                    answerModels[i].IsCorrect = answerModels[i].IsCorrect;
                }

                context.Answers.Add(answerModels[i]);
            }

            await context.SaveChangesAsync();

            // Increment the current question in session
            int? currentQuestion = HttpContext.Session.GetInt32("CurrentQuestion");
            HttpContext.Session.SetInt32("CurrentQuestion", currentQuestion.GetValueOrDefault() + 1);

            // Redirect to AddQuestions for the next question or finish the quiz
            int? totalQuestions = HttpContext.Session.GetInt32("TotalQuestions");

            if (currentQuestion >= totalQuestions)
            {
                // If all questions are added, redirect to the QuizComplete page
                return RedirectToAction("QuizComplete", new { quizId = question.QuizId });
            }

            return RedirectToAction("AddQuestions", new { quizId = question.QuizId });
        }

        // 4. Quiz Complete
        public IActionResult QuizComplete(int quizId)
        {
            var quiz = context.Quizzes.Find(quizId);
            if (quiz == null) return NotFound("Quiz not found.");

            ViewBag.QuizTitle = quiz.Title;
            ViewBag.CourseId = quiz.CourseId;

            return View();
        }

        // 5. EditQuiz
        [HttpGet]
        public async Task<IActionResult> EditQuiz(int quizId)
        {
            var quiz = await context.Quizzes.FindAsync(quizId);
            if (quiz == null) return NotFound("Quiz not found.");

            return View(quiz);
        }

        [HttpPost]
        public async Task<IActionResult> EditQuiz(QuizModel updatedQuiz)
        {
            if (!ModelState.IsValid)
            {
                return View(updatedQuiz);
            }

            var quiz = await context.Quizzes.FindAsync(updatedQuiz.QuizId);
            if (quiz == null) return NotFound("Quiz not found.");

            quiz.Title = updatedQuiz.Title;
            quiz.Duration = updatedQuiz.Duration;
            quiz.NumberOfQuestions = updatedQuiz.NumberOfQuestions;

            context.Quizzes.Update(quiz);
            await context.SaveChangesAsync();

            return RedirectToAction("QuizComplete", new { quizId = quiz.QuizId });
        }

        // 6. DeleteQuiz
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var quiz = await context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                return NotFound("Quiz not found.");
            }

            context.Quizzes.Remove(quiz);
            await context.SaveChangesAsync();

            return RedirectToAction("CourseConfig","Course", new { id = quiz.CourseId });
        }

        // 7. DetailsQuiz
        [HttpGet]
        public async Task<IActionResult> DetailsQuiz(int quizId)
        {
            var quiz = await context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.QuizId == quizId);

            if (quiz == null) return NotFound("Quiz not found.");

            return View(quiz);
        }


        public async Task<int> CalculateRemainingMarksAsync(int courseId)
        {
            var course = await context.Courses
                .Include(c => c.Quizzes)
                .ThenInclude(q => q.Questions)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null) return 100; // If course not found, return full marks

            int totalMarks = course.Quizzes
                .SelectMany(q => q.Questions)
                .Sum(q => q.Marks);

            return 100 - totalMarks; // Return the remaining marks
        }

    }
}
