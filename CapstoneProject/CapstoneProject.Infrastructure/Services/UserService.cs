using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Application.Interface.IService;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Infrastructure.Repostitory;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IAuthRepository _authRepository;

        public UserService(IUserRepository userRepository, IAuthRepository authRepository, IEmailService emailService)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _authRepository = authRepository;
        }

            public async Task<UserGetProfileRequest> GetProfileUser(int userId)
            {
                 var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new Exception("User not exit!");
                }

                var userProfile = new UserGetProfileRequest
                {
                    UserId = userId,
                    Email = user.Email,
                    AvatarUrl = user.AvatarUrl,
                    FullName = user.FullName,
                    Phone  = user.Phone,
                    Status = user.Status
                };

                return userProfile;
            }

            public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileRequest request)
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) throw new Exception("User do not found.");
                user.FullName = request.FullName;
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.PasswordHash); ;
                user.Phone = request.Phone;
                user.AvatarUrl = request.AvatarUrl;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
                return true;
            }



      
    }
}
