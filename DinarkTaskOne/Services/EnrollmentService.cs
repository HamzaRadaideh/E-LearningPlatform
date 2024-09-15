using DinarkTaskOne.Data;
using DinarkTaskOne.Models.ManageCourse;
using Microsoft.EntityFrameworkCore;

namespace DinarkTaskOne.Services
{
    public class EnrollmentService(ApplicationDbContext context) : IEnrollmentService
    {
        public async Task<bool> EnrollStudentAsync(int studentId, int courseId)
        {
            // Check if the course exists and is not full
            var course = await context.Courses.Include(c => c.Enrollments).FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null || course.Enrollments.Count >= course.MaxCapacity)
            {
                return false; // Course doesn't exist or is full
            }

            // Check if the student is already enrolled
            if (await IsStudentEnrolledAsync(studentId, courseId))
            {
                return false; // Student is already enrolled
            }

            // Create a new enrollment and set the appropriate fields
            var enrollment = new EnrollModel
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow, // Set the current timestamp
                Status = "Active" // Set the default status as Active
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
