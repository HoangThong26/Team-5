using CapstoneProject.Domain.Entities;

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

        Task<List<User>> GetStudentsAsync();
        Task<PasswordResetToken?> GetValidOtpByUserAsync(int userId, string otp);
        Task MarkOtpUsedAsync(PasswordResetToken token);
        Task RevokeAllRefreshTokensAsync(int userId);
        Task ChangePasswordAsync(int userId, string newPasswordHash);
        Task<List<User>> SearchUsersAsync(string keyword);

        Task<List<string>> GetAllEmailsAsync();
        Task AddRangeAsync(IEnumerable<User> users);

        Task<List<User>> GetMentorsWithGroupsAsync();
    }
}
