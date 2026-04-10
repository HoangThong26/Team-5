using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Infrastructure.Database.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Infrastructure.Repostitory
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
         .Include(u => u.GroupMember)
             .ThenInclude(gm => gm.Group)
                 .ThenInclude(g => g.FinalGrade)
         .ToListAsync();
        }

        public async Task UpdateUserStatusAsync(int userId, string newStatus)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Status = newStatus;
                await _context.SaveChangesAsync();
            }
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            var tokens = _context.RefreshTokens.Where(x => x.UserId == userId);
            _context.RefreshTokens.RemoveRange(tokens);

            var histories = _context.LoginHistories.Where(x => x.UserId == userId);
            _context.LoginHistories.RemoveRange(histories);

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();
        }

        public async Task ChangeStatusAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("User not exit!");
            }

            if (user.Status == "Active")
            {
                throw new Exception("Account do not locked");
            }
            user.Status = "Active";
            user.LockUntil = null;
            await _context.SaveChangesAsync();
        }



        public async Task<PasswordResetToken?> GetValidOtpByUserAsync(int userId, string otp)
        {
            return await _context.PasswordResetTokens
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.Token == otp &&
                    x.IsUsed == false &&
                    x.ExpiryTime > DateTime.UtcNow);
        }

        public async Task MarkOtpUsedAsync(PasswordResetToken token)
        {
            token.IsUsed = true;
            await _context.SaveChangesAsync();
        }

        public async Task RevokeAllRefreshTokensAsync(int userId)
        {
            var tokens = _context.RefreshTokens
                .Where(x => x.UserId == userId && x.IsRevoked == false);

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(int userId, string newPasswordHash)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            user.PasswordHash = newPasswordHash;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<List<User>> SearchUsersAsync(string keyword)
        {
            keyword = keyword.ToLower();

            return await _context.Users
                .Where(u =>
                    //u.FullName.ToLower().Contains(keyword) ||
                    u.Email.ToLower().Contains(keyword))
                .ToListAsync();
        }

        public async Task<List<string>> GetAllEmailsAsync()
        {
            return await _context.Users.Select(u => u.Email).ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<User> users)
        {
            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();
        }

        public async Task<List<User>> GetStudentsAsync()
        {
            return await _context.Users
                .Include(u => u.GroupMember)            // 1. Kết nối bảng trung gian GroupMember
                    .ThenInclude(gm => gm.Group)        // 2. Từ GroupMember lấy thông tin Groups
                        .ThenInclude(g => g.FinalGrade) // 3. Từ Groups lấy điểm FinalGrades
                .Where(u => u.Role == "Student")
                .ToListAsync();
        }
        public async Task<List<User>> GetMentorsWithGroupsAsync()
        {
            return await _context.Users
                .Include(u => u.MentorAssignments)
                    .ThenInclude(ma => ma.Group)
                .Where(u => u.Role == "Mentor")
                .ToListAsync();
        }



    }
}

