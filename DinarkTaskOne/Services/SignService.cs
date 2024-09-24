using DinarkTaskOne.Data;
using DinarkTaskOne.Models.Authentication_Authorization;
using DinarkTaskOne.Models.UserSpecficModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DinarkTaskOne.Services
{
    public class SignService(ApplicationDbContext context) : ISignService
    {
        public async Task RegisterUserAsync(UsersModel user, string password)
        {
            user.PasswordHash = HashPassword(password);

            if (user is InstructorModel instructor)
            {
                instructor.InstructorId = 0; // Ensure it's set to 0 so it will auto-increment in the database
                context.Instructors.Add(instructor);
            }
            else if (user is StudentModel student)
            {
                student.StudentId = 0; // Ensure it's set to 0 so it will auto-increment in the database
                context.Students.Add(student);
            }
            else
            {
                context.Users.Add(user);
            }

            await context.SaveChangesAsync();
        }


        // Fixing the method for validating a user by email and password
        public async Task<bool> ValidateUserAsync(string email, string password)
        {
            var user = await context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            // Avoiding possible null return by using null check
            if (user == null) return false;

            return VerifyPassword(password, user.PasswordHash);
        }

        // Fixing the method for validating a user by userId and password
        public async Task<bool> ValidateUserAsync(int userId, string password)
        {
            var user = await context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            // Avoiding possible null return by using null check
            if (user == null) return false;

            return VerifyPassword(password, user.PasswordHash);
        }

        public async Task<UsersModel?> GetUserByIdAsync(int userId)
        {
            var user = await context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            // If user is null, throw an exception or handle it appropriately
            return user ?? throw new Exception("User not found.");
        }

        public async Task<UsersModel?> GetUserByEmailAsync(string email)
        {
            var user = await context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            // If user is null, throw an exception or handle it appropriately
            return user ?? throw new Exception("User not found.");
        }

        public async Task UpdateUserAsync(UsersModel user)
        {
            context.Users.Update(user);
            await context.SaveChangesAsync();
        }

        public string HashPassword(string password)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hash);
        }

        public bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }
    }
}
