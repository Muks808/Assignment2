using Assignment2.Data;
using Assignment2.DTOs;
using Assignment2.Models;

namespace Assignment2.Services
{
    public class AuthService
    {
        private readonly RaceDayStore _store;
        private readonly PasswordService _passwordService;

        public AuthService(RaceDayStore store, PasswordService passwordService)
        {
            _store = store;
            _passwordService = passwordService;
        }

        public User? Register(RegisterDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            if (_store.Users.Any(u => u.Email == email))
                return null;

            if (!string.Equals(dto.Role, "Organiser", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(dto.Role, "Participant", StringComparison.OrdinalIgnoreCase))
                return null;

            var user = new User
            {
                UserID = _store.GetNextUserId(),
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = email,
                Phone = dto.Phone.Trim(),
                Role = char.ToUpper(dto.Role.Trim()[0]) + dto.Role.Trim()[1..].ToLowerInvariant(),
                CreatedAt = DateTime.UtcNow
            };

            user.Password = _passwordService.HashPassword(user, dto.Password);
            _store.Users.Add(user);

            if (user.Role == "Organiser")
            {
                _store.Organisers.Add(new Organiser
                {
                    OrganiserID = _store.GetNextOrganiserId(),
                    UserID = user.UserID,
                    OrganisationName = string.IsNullOrWhiteSpace(dto.OrganisationName)
                        ? $"{user.FirstName} {user.LastName} Events"
                        : dto.OrganisationName.Trim(),
                    ContactNumber = dto.Phone.Trim(),
                    Website = dto.Website?.Trim() ?? "",
                    CreatedAt = DateTime.UtcNow,
                    User = user
                });
            }
            else
            {
                _store.Participants.Add(new Participant
                {
                    ParticipantID = _store.GetNextParticipantId(),
                    UserID = user.UserID,
                    DateOfBirth = dto.DateOfBirth ?? new DateTime(2000, 1, 1),
                    Gender = dto.Gender?.Trim() ?? "Not specified",
                    EmergencyContact = dto.EmergencyContact?.Trim() ?? dto.Phone.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    User = user
                });
            }

            return user;
        }

        public User? Login(LoginDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var user = _store.Users.FirstOrDefault(u => u.Email == email);

            if (user == null || string.IsNullOrWhiteSpace(user.Password))
                return null;

            return _passwordService.VerifyPassword(user, user.Password, dto.Password)
                ? user
                : null;
        }
    }
}