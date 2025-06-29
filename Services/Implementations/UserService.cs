using System.Security.Cryptography;
using TravelBookingApi.Models.DTOs;
using TravelBookingApi.Models.Entities;
using TravelBookingApi.Repositories.Interfaces;
using TravelBookingApi.Services.Interfaces;
using TravelBookingApi.Utilities.Helpers;

namespace TravelBookingApi.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public UserService(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<List<UserDTO>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(u => MapToDTO(u)).ToList();
        }

        public async Task<UserDTO> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return MapToDTO(user);
        }

        public async Task<string> RegisterAsync(UserRegisterDTO registerDto)
        {
            if (await _userRepository.UserExistsAsync(registerDto.Username))
                throw new Exception("Username already exists");

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = PasswordHelper.HashPassword(registerDto.Password),
                FullName = registerDto.FullName,
                Role = registerDto.Role ?? "User",
                AuthToken = null,
                TokenExpiry = null
            };

            await _userRepository.AddAsync(user);
            return $"User registered successfully as {user.Role}";
        }

        public async Task<LoginResponseDTO> LoginAsync(UserLoginDTO loginDto)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(loginDto.Username) ||
                string.IsNullOrWhiteSpace(loginDto.Password))
            {
                throw new ArgumentException("Username and password are required");
            }

            // Get user from database
            var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
            if (user == null)
            {
                // Simulate password verification to prevent timing attacks
                PasswordHelper.VerifyPassword("dummy_password",
                    "dummy_hashed_password_that_will_fail_verification");
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            // Verify password
            if (!PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            // Generate new token
            user.AuthToken = _tokenService.GenerateToken(user.Username);
            user.TokenExpiry = DateTime.UtcNow.AddHours(2);

            try
            {
                await _userRepository.UpdateAsync(user);
            }
            catch
            {
                throw new Exception("Failed to update user token");
            }

            return new LoginResponseDTO
            {
                Token = user.AuthToken,
                Role = user.Role,
                Username = user.Username,
                Expiry = user.TokenExpiry
            };
        }
        public async Task UpdateAsync(int id, UserUpdateDTO updateDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) throw new Exception("User not found");

            if (!string.IsNullOrEmpty(updateDto.Email))
                user.Email = updateDto.Email;

            if (!string.IsNullOrEmpty(updateDto.FullName))
                user.FullName = updateDto.FullName;

            if (!string.IsNullOrEmpty(updateDto.Password))
                user.PasswordHash = PasswordHelper.HashPassword(updateDto.Password);

            await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) throw new Exception("User not found");

            await _userRepository.DeleteAsync(user);
        }

        private UserDTO MapToDTO(User user)
        {
            return new UserDTO
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role
            };
        }
    }
}