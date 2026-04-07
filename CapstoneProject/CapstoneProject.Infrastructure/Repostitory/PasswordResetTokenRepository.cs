using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Infrastructure.Database.AppDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Infrastructure.Repostitory
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public PasswordResetTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PasswordResetToken token)
        {
            await _context.PasswordResetTokens.AddAsync(token);
        }

        public async Task<PasswordResetToken?> GetValidTokenAsync(string email, string token)
        {
            return await _context.PasswordResetTokens
                .Include(x => x.User)
                .Where(x => x.User.Email == email
                         && x.Token == token
                         && x.IsUsed == false
                         && x.ExpiryTime > DateTime.UtcNow)
                .FirstOrDefaultAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
