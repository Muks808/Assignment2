using Assignment2.Models;
using Microsoft.AspNetCore.Identity;

namespace Assignment2.Services
{
    public class PasswordService
    {
        private readonly PasswordHasher<User> _hasher = new();

        public string HashPassword(User user, string password)
        {
            return _hasher.HashPassword(user, password);
        }

        public bool VerifyPassword(User user, string passwordHash, string password)
        {
            var result = _hasher.VerifyHashedPassword(user, passwordHash, password);
            return result != PasswordVerificationResult.Failed;
        }
    }
}
