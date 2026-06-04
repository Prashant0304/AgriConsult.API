using AgriConsult.API.Models;

namespace AgriConsult.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<User> CreateUserAsync(string fullName, string email, string password);
    }
}
