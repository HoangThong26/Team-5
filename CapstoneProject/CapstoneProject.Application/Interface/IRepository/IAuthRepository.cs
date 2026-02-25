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
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task<User?> GetByVerifyTokenAsync(string token);
    }
}
