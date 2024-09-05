using DinarkTaskOne.Data;
using DinarkTaskOne.Models.ManageCourse;
using Microsoft.EntityFrameworkCore;

namespace DinarkTaskOne.Services
{
    public class EnrollmentService(ApplicationDbContext context) : IEnrollmentService
    {
        public async Task<bool> EnrollStudentAsync(int studentId, int courseId)
        {
            // Check if the student is already enrolled
            if (await IsStudentEnrolledAsync(studentId, courseId))
            {
                return false; // Student is already enrolled
            }

            var enrollment = new EnrollModel
            {
                StudentId = studentId,
                CourseId = courseId
            };

            context.Enrollments.Add(enrollment);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsStudentEnrolledAsync(int studentId, int courseId)
        {
            return await context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
        }

        public async Task RemoveEnrollmentAsync(int enrollmentId)
        {
            var enrollment = await context.Enrollments.FindAsync(enrollmentId);
            if (enrollment != null)
            {
                context.Enrollments.Remove(enrollment);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<EnrollModel>> GetEnrollmentsByStudentIdAsync(int studentId)
        {
            return await context.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Instructor)
                .Where(e => e.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<bool> CourseExistsAsync(int courseId)
        {
            return await context.Courses.AnyAsync(c => c.CourseId == courseId);
        }
    }
}
