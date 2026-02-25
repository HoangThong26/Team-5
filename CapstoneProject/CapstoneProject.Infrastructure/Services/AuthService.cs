using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Application.Interface.IService;
using CapstoneProject.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace CapstoneProject.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IAuthRepository authRepository, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _configuration = configuration;
        }

        public async Task<string> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _authRepository.GetByEmailAsync(request.Email);

            if (existingUser != null)
            {
                if (existingUser.EmailVerified == true)
                    throw new Exception("Email already exists!");

                existingUser.VerifyToken = Guid.NewGuid().ToString();
                existingUser.VerifyTokenExpire = DateTime.UtcNow.AddHours(24);

                await _authRepository.UpdateAsync(existingUser);
                await SendVerifyEmail(existingUser.Email, existingUser.VerifyToken);

                return "Verification email resent.";
            }

            var token = Guid.NewGuid().ToString();

            var user = new User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                Phone = request.Phone,
                EmailVerified = false,
                Role = "Student",
                VerifyToken = token,
                VerifyTokenExpire = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow
            };

            await _authRepository.AddAsync(user);
            await SendVerifyEmail(user.Email, token);

            return "Register successfully. Please verify your email.";
        }

        private async Task SendVerifyEmail(string email, string token)
        {
            var link = $"https://localhost:7084/api/registers/verify?token={token}";

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("he186507hoangbathong@gmail.com", "hthjxjajxbgrwljo"),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress("he186507hoangbathong@gmail.com"),
                Subject = "Verify your account",
                IsBodyHtml = true,
                Body = $@"
                    <h2>Verify Your Account</h2>
                    <p>Click the button below to verify your account:</p>
                   <button> <a href='{link}' 
                     style='padding:10px 20px;
                      background-color:blue;
                      color:white;
                      text-decoration:none;
                      border-radius:5px;'>
                        Verify Email
                    </a>
                </button>
                    <p>Or copy this link:</p>
                    <p>{link}</p>"
            };

            mail.To.Add(email);

            await client.SendMailAsync(mail);
        }

        public async Task<string> VerifyAsync(string token)
        {
            var user = await _authRepository.GetByVerifyTokenAsync(token);

            if (user == null)
                return "Invalid token";

            if (user.VerifyTokenExpire < DateTime.UtcNow)
                return "Token expired";

            if (user.EmailVerified == true)
                return "Account already verified";

            user.EmailVerified = true;
            user.VerifyToken = null;
            user.VerifyTokenExpire = null;

            await _authRepository.UpdateAsync(user);

            return "Email verified successfully!";
        }

    }
}