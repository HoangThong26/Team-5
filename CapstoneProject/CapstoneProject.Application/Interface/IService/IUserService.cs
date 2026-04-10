using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.Interface.IService
{
    public interface IUserService
    {
        Task<UserGetProfileRequest> GetProfileUser(int userId);
        Task<bool> UpdateProfileAsync(int userId, UpdateProfileRequest request);
        Task UpdatePasswordProfileAsync(int userId, UpdatePasswordProfileRequest request);

        Task SendOtpForChangePasswordAsync(int userId);
        Task<List<AdminUserResponse>> SearchUsersAsync(string? keyword, string? role, string? status);
    }
}
