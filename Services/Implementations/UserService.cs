using AgriConsult.API.Data;
using AgriConsult.API.Models;
using AgriConsult.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgriConsult.API.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;

        // Dependency injected — DIP applied here too
        public UserService(AppDbContext db)
        {
            _db = db;
        }

        // ONE job: find user by email
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _db.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        // ONE job: check if email exists
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _db.Users
                .AnyAsync(u => u.Email == email);
        }

        // ONE job: create new user with hashed password
        public async Task<User> CreateUserAsync(
            string fullName, string email, string password)
        {
            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "Farmer"
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }
    }
}