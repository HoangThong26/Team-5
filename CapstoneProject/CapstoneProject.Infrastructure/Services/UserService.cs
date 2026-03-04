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
        private readonly IPasswordResetTokenRepository _tokenRepository;

        public UserService(IUserRepository userRepository, IAuthRepository authRepository, IEmailService emailService, IPasswordResetTokenRepository tokenRepository)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _authRepository = authRepository;
            _tokenRepository = tokenRepository;
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
                user.Phone = request.Phone;
                user.AvatarUrl = request.AvatarUrl;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
                return true;
            }




        public async Task UpdatePasswordProfileAsync(
     int userId,
     UpdatePasswordProfileRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            // 1️⃣ Check old password
            bool isMatch = BCrypt.Net.BCrypt.Verify(
                request.OldPassword,
                user.PasswordHash);

            if (!isMatch)
                throw new Exception("Old password is incorrect");

            // 2️⃣ Check OTP
            var validOtp = await _userRepository
                .GetValidOtpByUserAsync(userId, request.Otp);

            if (validOtp == null)
                throw new Exception("Invalid or expired OTP");

            // 3️⃣ Không cho trùng mật khẩu cũ
            if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
                throw new Exception("New password must be different from old password");

            // 4️⃣ Hash mật khẩu mới
            string newPasswordHash =
                BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // 5️⃣ Update password
            user.PasswordHash = newPasswordHash;

            // 6️⃣ Mark OTP used
            validOtp.IsUsed = true;

            // 7️⃣ Revoke toàn bộ refresh token
            await _userRepository.RevokeAllRefreshTokensAsync(userId);

            // 8️⃣ SaveChanges 1 lần duy nhất
            await _userRepository.SaveChangesAsync();
        }


        public async Task SendOtpForChangePasswordAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            var otp = new Random().Next(100000, 999999).ToString();

            var token = new PasswordResetToken
            {
                UserId = userId,
                Token = otp,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            };

            await _tokenRepository.AddAsync(token);
            await _tokenRepository.SaveChangesAsync();

            await _emailService.SendEmailAsync(user.Email, "Your OTP", $"Your OTP is: {otp}");
        }



    }
}
