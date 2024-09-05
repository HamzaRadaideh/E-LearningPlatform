using DinarkTaskOne.Models.ManageCourse;

namespace DinarkTaskOne.Services
{
    public interface IEnrollmentService
    {
        Task<bool> EnrollStudentAsync(int studentId, int courseId);
        Task<bool> IsStudentEnrolledAsync(int studentId, int courseId);
        Task RemoveEnrollmentAsync(int enrollmentId);
        Task<IEnumerable<EnrollModel>> GetEnrollmentsByStudentIdAsync(int studentId);
        Task<bool> CourseExistsAsync(int courseId);
    }
}