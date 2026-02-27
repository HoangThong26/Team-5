using CapstoneProject.Application.DTO;
using CapstoneProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.Interface.IRepository
{
    public interface IAuthRepository
    {
        Task<User> GetByEmailAsync(string email);
        Task<User?>GetByIdAsync(int userId);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task<User?> GetByVerifyTokenAsync(string token);
        Task AddRefreshTokenAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
        Task RevokeRefreshTokenAsync(string token);
        Task SaveLoginHistoryAsync(LoginHistory history);
        Task<PasswordResetToken?> GetValidTokenAsync(string email, string otp);
        Task UpdateStatusByRefreshTokenAsync(string refreshToken, string newStatus);



    }
}
