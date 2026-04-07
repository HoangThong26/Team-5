using CapstoneProject.Application.DTO;
using CapstoneProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.Interface.IService
{
    public interface IAuthService
    {
        Task<TokenResponse> RefreshTokenAsync(string refreshToken);
        Task<bool> LogoutAsync(string refreshToken);
        Task<LoginResponse> LoginAsync(LoginRequest request, string ipAddress);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(ResetPasswordRequestDto request);

    }
}
