using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Infrastructure.Database.AppDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

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
            return await _context.Users.ToListAsync();
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
            user.Status = "Active";
            user.LockUntil = null;
            await _context.SaveChangesAsync();
        }
    }
}
