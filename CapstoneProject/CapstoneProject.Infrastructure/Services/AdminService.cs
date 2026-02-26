using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
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
            if (existing != null) throw new Exception("Email đã tồn tại trên hệ thống.");
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
    }
}
