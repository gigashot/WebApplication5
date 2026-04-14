using System;
using System.Linq;
using System.Threading.Tasks;
using WebApplication5.Models;
using WebApplication5.Models.DTOs;
using WebApplication5.Models.Entities;
using WebApplication5.Repositories;
using WebApplication5.Repositories.Interfaces;
using WebApplication5.Services.Interfaces;

namespace WebApplication5.Services
{
    public class AuthService : IAuthService
    {
        private readonly EncryptAppDbContext _context;
        private readonly IUserRepository _userRepository;

        public AuthService()
        {
            _context = new EncryptAppDbContext();
            _userRepository = new UserRepository(_context);
        }

        public async Task<ApiResponse> Register(RegisterDto dto)
        {
            var passwordError = ValidatePassword(dto.Password);
            if (passwordError != null)
            {
                return ApiResponse.Error(passwordError);
            }

            if (await _userRepository.UsernameExists(dto.Username))
            {
                return ApiResponse.Error("Username already exists.");
            }

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                PublicKey = dto.PublicKey,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.Create(user);

            return ApiResponse.Ok("Registration successful.");
        }

        public async Task<ApiResponse<LoginResponseDto>> Login(LoginDto dto)
        {
            var user = await _userRepository.GetByUsername(dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return new ApiResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = "Invalid username or password."
                };
            }

            await _userRepository.UpdateLastLogin(user.UserId);

            var response = new LoginResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                PublicKey = user.PublicKey
            };

            return ApiResponse<LoginResponseDto>.Ok(response, "Login successful.");
        }

        private string ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return "Password must be at least 8 characters.";
            if (!password.Any(char.IsUpper))
                return "Password must contain at least one uppercase letter.";
            if (!password.Any(char.IsLower))
                return "Password must contain at least one lowercase letter.";
            if (!password.Any(char.IsDigit))
                return "Password must contain at least one digit.";
            if (!password.Any(c => !char.IsLetterOrDigit(c)))
                return "Password must contain at least one special character.";
            return null;
        }
    }
}
