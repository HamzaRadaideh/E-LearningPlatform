using DinarkTaskOne.Data;
using DinarkTaskOne.Models.Authentication_Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DinarkTaskOne.Services
{
    public class SignService(ApplicationDbContext context) : ISignService
    {
        public async Task RegisterUserAsync(UsersModel user, string password)
        {
            user.PasswordHash = HashPassword(password);
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        public async Task<bool> ValidateUserAsync(string email, string password)
        {
            var user = await context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            return user != null && VerifyPassword(password, user.PasswordHash);
        }

        public async Task<bool> ValidateUserAsync(int userId, string password)
        {
            var user = await context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            return user != null && VerifyPassword(password, user.PasswordHash);
        }

        public async Task<UsersModel> GetUserByIdAsync(int userId)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);
#pragma warning restore CS8603 // Possible null reference return.
        }

        public async Task<UsersModel> GetUserByEmailAsync(string email)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
#pragma warning restore CS8603 // Possible null reference return.
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
