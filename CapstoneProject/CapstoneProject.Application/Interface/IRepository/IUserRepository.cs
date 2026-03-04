using CapstoneProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.Interface.IRepository
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task<List<User>> GetAllUsersAsync();
        Task UpdateUserStatusAsync(int userId, string newStatus);
        Task DeleteAsync(int userId);
        Task ChangeStatusAsync(int userId);

        Task SaveChangesAsync();


        Task<PasswordResetToken?> GetValidOtpByUserAsync(int userId, string otp);
        Task MarkOtpUsedAsync(PasswordResetToken token);
        Task RevokeAllRefreshTokensAsync(int userId);
        Task ChangePasswordAsync(int userId, string newPasswordHash);
    }
}
