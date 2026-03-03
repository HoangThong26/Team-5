using CapstoneProject.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.Interface.IService
{
    public interface IUserService
    {
        Task<UserGetProfileRequest> GetProfileUser(int userId);
        Task<bool> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    }
}
