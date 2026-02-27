using CapstoneProject.Application.DTO;
using CapstoneProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.Interface.IService
{
    public interface IAdminService
    {
        Task<string> CreateUserByRoleAsync(AdminCreateUserRequest request);
        Task<List<User>> GetAllUsersAsync();
    }
}
