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
<<<<<<< HEAD

=======
>>>>>>> c461d4c1f68f0a909524422d02ea522b4ad20704
        Task AddRefreshTokenAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
        Task RevokeRefreshTokenAsync(string token);
<<<<<<< HEAD
=======
        Task SaveLoginHistoryAsync(LoginHistory history);
        Task<PasswordResetToken?> GetValidTokenAsync(string email, string otp);
        Task UpdateStatusByRefreshTokenAsync(string refreshToken, string newStatus);



>>>>>>> c461d4c1f68f0a909524422d02ea522b4ad20704
    }
}
