using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Application.Interface.IService;
using CapstoneProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Infrastructure.Repostitory
{
   

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

            public async Task DeleteAsync(int userId)
            {
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                    throw new Exception("User not found");

                await _userRepository.DeleteAsync(userId);
            }

            public async Task<List<User>> GetAllUsersAsync()
            {
                return await _userRepository.GetAllUsersAsync();
            }

            public async Task UnlockAccountAsync(int userId)
            {
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                    throw new Exception("User not found");

                if (user.LockUntil == null || user.LockUntil <= DateTime.UtcNow)
                    throw new Exception("Account is not locked");

                user.LockUntil = null;
                user.FailedLoginCount = 0;
                user.Status = "Active";
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
            }

            public async Task<List<AdminUserResponse>> SearchUsersAsync(string keyword)
            {
                var users = await _userRepository.SearchUsersAsync(keyword);

                return users.Select(u => new AdminUserResponse
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    Status = u.Status,
                    Role = u.Role
                }).ToList();
            }
        }
    }

}
