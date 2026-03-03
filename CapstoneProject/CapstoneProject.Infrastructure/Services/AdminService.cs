using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Application.Interface.IService;

namespace CapstoneProject.Infrastructure.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;

        public AdminService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<string> CreateUserByRoleAsync(AdminCreateUserRequest request)
        {
            var existing = await _userRepository.GetByEmailAsync(request.Email);
            if (existing != null) throw new Exception("Email Exited.");
            var newUser = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                Phone = request.Phone,
                Role = request.Role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Status = "Active",
                EmailVerified = true, 
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(newUser);

            return $"Created.";
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task DeleteAsync(int userId)
        {
            await _userRepository.DeleteAsync(userId);
        }

        public async Task UnlockAccountAsync(int userId)
        {
            await _userRepository.ChangeStatusAsync(userId);
        }
    }
}
