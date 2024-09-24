using DinarkTaskOne.Models.Authentication_Authorization;

namespace DinarkTaskOne.Services
{
    public interface ISignService
    {
        Task RegisterUserAsync(UsersModel user, string password);
        Task<bool> ValidateUserAsync(string email, string password);
        Task<UsersModel?> GetUserByIdAsync(int userId);
        Task<UsersModel?> GetUserByEmailAsync(string email);
        Task UpdateUserAsync(UsersModel user);
        string HashPassword(string password);
        bool VerifyPassword(string password, string storedHash);
        Task<bool> ValidateUserAsync(int userIdInt, string password);
    }
}