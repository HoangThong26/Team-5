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

     
    }
}
