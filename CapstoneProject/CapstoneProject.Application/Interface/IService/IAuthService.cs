using CapstoneProject.Application.DTO;
using CapstoneProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.Interface.IService
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterRequest request);
        Task<string> VerifyAsync(string token);
    }
}
