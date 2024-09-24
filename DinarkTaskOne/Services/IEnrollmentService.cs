using DinarkTaskOne.Models.ManageCourse;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DinarkTaskOne.Services
{
    public interface IEnrollmentService
    {
        // Enroll a student in a course
        Task<bool> EnrollStudentAsync(int studentId, int courseId);

        // Check if a student is allowed to enroll in a course based on major and level
        Task<bool> IsStudentAllowedAsync(int studentId, int courseId);

        // Check if a student is enrolled in a course
        Task<bool> IsStudentEnrolledAsync(int studentId, int courseId);

        // Get all enrollments for a specific student
        Task<IEnumerable<EnrollModel>> GetEnrollmentsByStudentIdAsync(int studentId);

        // Remove a student's enrollment from a course
        Task RemoveEnrollmentAsync(int enrollmentId);

        // Check if a course exists
        Task<bool> CourseExistsAsync(int courseId);

        // Calculate the average score for a student in a specific level
        Task<double> CalculateLevelAverageAsync(int studentId, int levelId);

        // Check if a student can advance to the next level
        Task<bool> CanAdvanceToNextLevelAsync(int studentId, int currentLevelId);

        // Update the student's level
        Task UpdateStudentLevelAsync(int studentId, int newLevelId);

        // Record student progress in a course and update grade if necessary
        Task RecordStudentProgressAsync(int studentId, int courseId, double score);
    }
}
