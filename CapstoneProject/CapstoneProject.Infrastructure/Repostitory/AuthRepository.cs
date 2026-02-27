using System;
using System.Collections.Generic;
using System.Text;
using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Infrastructure.Database.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Infrastructure.Repostitory
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _context;
        public AuthRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }
        public async Task AddAsync(User user)
        {
            var checkExit = await _context.Users.AnyAsync(u=> u.Email == user.Email);
            if (checkExit == true)
            {
                throw new Exception("Email already Exit");
            }
            await _context.Users.AddAsync(user);
           await _context.SaveChangesAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        public async Task<User?> GetByVerifyTokenAsync(string token)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.VerifyToken == token);
        }

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            if (refreshToken.IsRevoked == null) refreshToken.IsRevoked = false;

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task RevokeRefreshTokenAsync(string token)
        { 
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == token);

            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                _context.RefreshTokens.Update(refreshToken);
                await _context.SaveChangesAsync();
            }
        }

            public async Task SaveLoginHistoryAsync(LoginHistory history)
            {
                try
                {
                    await _context.LoginHistories.AddAsync(history);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                        throw new Exception("Failed to save login history: " + ex.Message);
            }
            }

        public async Task SaveResetTokenAsync(PasswordResetToken token)
        {
            await _context.PasswordResetTokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task<PasswordResetToken?> GetValidTokenAsync(string email, string otp)
        {
            return await _context.PasswordResetTokens
                .Include(t => t.User) 
                .Where(t => t.User.Email == email
                         && t.Token == otp
                         && t.IsUsed == false
                         && t.ExpiryTime > DateTime.UtcNow)
                .OrderByDescending(t => t.ExpiryTime)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateStatusByRefreshTokenAsync(string refreshToken, string newStatus)
        {
            var tokenRecord = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (tokenRecord != null)
            {
                var user = await _context.Users.FindAsync(tokenRecord.UserId);
                if (user != null)
                {
                    user.Status = newStatus;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task UpdateResetTokenAsync(PasswordResetToken token)
        {
            _context.PasswordResetTokens.Update(token);
            await _context.SaveChangesAsync();
        }
    }


    }

