using DinarkTaskOne.Data;
using DinarkTaskOne.Models.ManageCourse;
using DinarkTaskOne.Models.UserSpecficModels;
using DinarkTaskOne.Models.student;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DinarkTaskOne.Services
{
    public class EnrollmentService(ApplicationDbContext context) : IEnrollmentService
    {
        // Enroll a student in a course
        public async Task<bool> EnrollStudentAsync(int studentId, int courseId)
        {
            var course = await context.Courses
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null || course.Status != StatusType.Active || course.Enrollments.Count >= course.MaxCapacity)
            {
                return false;
            }

            if (!context.Levels.Any(l => l.LevelId == course.LevelId))
            {
                throw new Exception("Course LevelId does not exist in Levels table.");
            }

            // Check if the student is already enrolled or allowed in the course
            if (await IsStudentEnrolledAsync(studentId, courseId) || !await IsStudentAllowedAsync(studentId, courseId))
            {
                return false;
            }

            // Enroll student
            var enrollment = new EnrollModel
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow,
                Status = "Active"
            };

            await context.Enrollments.AddAsync(enrollment);
            await context.SaveChangesAsync();

            // Record initial student progress in the course
            await RecordStudentProgressAsync(studentId, courseId, 0); // Initial score is 0

            return true;
        }

        // Check if a student is allowed to enroll in a course based on major and level
        public async Task<bool> IsStudentAllowedAsync(int studentId, int courseId)
        {
            var student = await context.Students
                .Include(s => s.Major)
                .Include(s => s.CurrentLevel)
                .FirstOrDefaultAsync(s => s.UserId == studentId);

            var course = await context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (student == null || course == null)
            {
                return false;
            }

            var allowedMajors = course.AllowedMajors
                .Split(',')
                .Select(int.Parse)
                .ToList();

            // Check if student's major is allowed and if they are in the correct level for the course
            return allowedMajors.Contains(student.MajorId) && student.CurrentLevelId == course.LevelId;
        }

        // Check if a student is already enrolled in a course
        public async Task<bool> IsStudentEnrolledAsync(int studentId, int courseId)
        {
            return await context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
        }

        // Get all enrollments for a specific student
        public async Task<IEnumerable<EnrollModel>> GetEnrollmentsByStudentIdAsync(int studentId)
        {
            return await context.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Instructor)
                .Where(e => e.StudentId == studentId)
                .ToListAsync();
        }

        // Remove a student's enrollment from a course
        public async Task RemoveEnrollmentAsync(int enrollmentId)
        {
            var enrollment = await context.Enrollments.FindAsync(enrollmentId);
            if (enrollment != null)
            {
                context.Enrollments.Remove(enrollment);
                await context.SaveChangesAsync();
            }
        }

        // Check if a course exists
        public async Task<bool> CourseExistsAsync(int courseId)
        {
            return await context.Courses.AnyAsync(c => c.CourseId == courseId);
        }

        // Calculate the average score for a student in a specific level
        public async Task<double> CalculateLevelAverageAsync(int studentId, int levelId)
        {
            var progress = await context.StudentProgresses
                .Where(sp => sp.StudentId == studentId && sp.LevelId == levelId)
                .ToListAsync();

            return progress.Count > 0 ? progress.Average(sp => sp.Score) : 0.0;
        }

        // Check if a student can advance to the next level
        public async Task<bool> CanAdvanceToNextLevelAsync(int studentId, int currentLevelId)
        {
            var student = await context.Students.FindAsync(studentId);
            if (student == null) return false;

            // Check if the student has completed all courses in the current level
            var levelCourses = await context.Courses
                .Where(c => c.LevelId == currentLevelId)
                .Include(c => c.Enrollments)
                .ToListAsync();

            if (levelCourses.Any(course => !course.Enrollments.Any(e => e.StudentId == studentId && e.Status == "Passed")))
            {
                return false;
            }

            // Check if the next level exists
            var nextLevel = await context.Levels
                .Where(l => l.LevelId > currentLevelId)
                .OrderBy(l => l.LevelId)
                .FirstOrDefaultAsync();

            return nextLevel != null;
        }

        // Update the student's level
        public async Task UpdateStudentLevelAsync(int studentId, int newLevelId)
        {
            var student = await context.Students.FindAsync(studentId);
            if (student == null) return;

            student.CurrentLevelId = newLevelId;
            context.Students.Update(student);
            await context.SaveChangesAsync();
        }

        // Record student progress in a course and update grade if necessary
        public async Task RecordStudentProgressAsync(int studentId, int courseId, double score)
        {
            // Retrieve the levelId from the course
            var course = await context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
            {
                throw new Exception("Course not found.");
            }

            int levelId = course.LevelId; // Correctly retrieve the LevelId from the course

            var existingProgress = await context.StudentProgresses
                .FirstOrDefaultAsync(sp => sp.StudentId == studentId && sp.LevelId == levelId);

            if (existingProgress != null)
            {
                existingProgress.Score = score;
                existingProgress.IsCompleted = true;
                existingProgress.CompletedAt = DateTime.UtcNow;
                context.StudentProgresses.Update(existingProgress);
            }
            else
            {
                var newProgress = new StudentProgressModel
                {
                    StudentId = studentId,
                    LevelId = levelId, // Use the correct LevelId from the course
                    Score = score,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow
                };
                context.StudentProgresses.Add(newProgress);
            }

            await context.SaveChangesAsync();

            // Update the student's average score for the level
            var averageScore = await CalculateLevelAverageAsync(studentId, levelId);
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
                    CalculatedAt = DateTime.UtcNow
                };
                context.StudentGrades.Add(studentGrade);
            }
            else
            {
                studentGrade.AverageScore = averageScore;
                studentGrade.HasPassed = averageScore >= 50;
                studentGrade.CalculatedAt = DateTime.UtcNow;
                context.StudentGrades.Update(studentGrade);
            }

            await context.SaveChangesAsync();
        }

    }
}
