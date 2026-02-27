using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Application.Interface.IService;
using CapstoneProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
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
        private readonly IEmailService _emailService;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenRepository _tokenRepository;
        private readonly IConfiguration _config;

        public AuthService(IAuthRepository authRepository, IConfiguration configuration, IEmailService emailService
            ,IPasswordResetTokenRepository passwordResetTokenRepository, IUserRepository userRepository, IConfiguration config)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _emailService = emailService;
            _userRepository = userRepository;
            _tokenRepository = passwordResetTokenRepository;
            _config = config;
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
                Status = "Active",
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
            var baseUrl = _config["AppSettings:BaseUrl"];
            var senderEmail = _config["EmailSettings:SenderEmail"];
            var appPassword = _config["EmailSettings:AppPassword"];

            var link = $"{baseUrl}/api/auth/verify?token={token}";

            using var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(senderEmail, appPassword),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail, "Capstone Management"),
                Subject = "Capstone Management - Verify Email",
                IsBodyHtml = true,
                Body = $@"
                <div style='font-family:Arial,sans-serif;line-height:1.6;color:#333'>
                <h2>Hello,</h2>

                <p>You have just registered an account on the <b>Capstone Management</b> system.</p>

                <p>To complete your registration, please confirm your email address by clicking the button below:</p>

                    <div style='margin:20px 0'>
                 <a href='{link}' target='_blank'
                    style='display:inline-block;
                  padding:12px 24px;
                  background-color:#007bff;
                  color:#ffffff;
                  text-decoration:none;
                  border-radius:6px;
                  font-weight:bold;'>
                    Verify Email
                        </a>
                    </div>

                <p>If you did not make this request, please ignore this email.</p>

                <hr style='margin:30px 0'>

                <p style='font-size:12px;color:#888'>
                    Capstone Management System<br/>
                    This is an automated email. Please do not reply directly.
                </p>

                <p style='font-size:12px;color:#888'>
                    Or copy and paste the following link into your browser:<br/>
                    {link}
                </p>
            </div>"
            };

            mail.To.Add(email);

            mail.ReplyToList.Add(senderEmail);
            mail.Headers.Add("X-Priority", "3");

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

        public async Task<TokenResponse> LoginAsync(LoginRequest request, string ipAddress)
        {
            var user = await _authRepository.GetByEmailAsync(request.Email);
            bool isSuccess = false;
            async Task LogHistory(int? uid, bool success)
            {
                var history = new LoginHistory
                {
                    UserId = uid,
                    Ipaddress = ipAddress,
                    LoginTime = DateTime.UtcNow,
                    IsSuccess = success
                };
                await _authRepository.SaveLoginHistoryAsync(history); 
            }
            if (user == null)
            {
                await LogHistory(null, false); 
                throw new Exception("Invalid email or password.");
            }

            if (user.LockUntil.HasValue && user.LockUntil > DateTime.UtcNow)
            {
                await LogHistory(user.UserId, false); 
                var remainingMinutes = Math.Ceiling((user.LockUntil.Value - DateTime.UtcNow).TotalMinutes);
                throw new Exception($"Account is temporarily locked. Try again after {remainingMinutes} minutes.");
            }
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                user.FailedLoginCount++;
                if (user.FailedLoginCount >= 5)
                {
                    int lockMinutes = ((user.FailedLoginCount ?? 0) - 4) * 5;
                    user.LockUntil = DateTime.UtcNow.AddMinutes(lockMinutes);
                    user.Status = "Locked";
                }
                await _authRepository.UpdateAsync(user);

                await LogHistory(user.UserId, false);
                throw new Exception("Invalid email or password.");
            }

            if (user.EmailVerified == false)
            {
                await LogHistory(user.UserId, false);
                throw new Exception("Email address is not verified.");
            }
            user.FailedLoginCount = 0;
            user.LockUntil = null;
            user.Status = "Active";
            await _authRepository.UpdateAsync(user);

            await LogHistory(user.UserId, true);
            return await GenerateTokens(user);
        }

        public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _authRepository.GetRefreshTokenAsync(refreshToken);

            if (storedToken == null)
                throw new Exception("Refresh Token do not exit.");

            if (storedToken.IsRevoked == true)
                throw new Exception("Refresh Token revoked.");

            if (storedToken.ExpiryDate < DateTime.UtcNow)
                throw new Exception("Refresh Token expiryDate.");
            storedToken.IsRevoked = true;
            await _authRepository.UpdateRefreshTokenAsync(storedToken);
            return await GenerateTokens(storedToken.User);
        }
        private async Task<TokenResponse> GenerateTokens(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey)) throw new Exception("Jwt:Key is null or empty");

            var jwtHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Role, user.Role ?? "Student")
        }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtHandler.CreateToken(tokenDescriptor);
            var accessToken = jwtHandler.WriteToken(token);
            var newRefreshToken = new RefreshToken
            {
                UserId = user.UserId,
                User = user, 
                Token = Guid.NewGuid().ToString() + "-" + DateTime.UtcNow.Ticks,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };
            if (_authRepository == null) throw new Exception("_authRepository is null");

            await _authRepository.AddRefreshTokenAsync(newRefreshToken);

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiryDate = newRefreshToken.ExpiryDate,
                User = new UserViewDTO
                {
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role
                }
            };
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken)) return false;
            await _authRepository.UpdateStatusByRefreshTokenAsync(refreshToken, "Inactive");
            await _authRepository.RevokeRefreshTokenAsync(refreshToken);

            return true;
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
                throw new Exception("Email do not exited!");
            var otp = new Random().Next(100000, 999999).ToString();

            var resetToken = new PasswordResetToken
            {
                UserId = user.UserId,
                Token = otp,
                ExpiryTime = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            await _tokenRepository.AddAsync(resetToken);
            await _tokenRepository.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                user.Email,
                "Reset Password",
                $"OTP: {otp}"
            );
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var token = await _tokenRepository
                .GetValidTokenAsync(request.Email, request.Token);

            if (token == null)
                throw new Exception("OTP Invalid or expired");

            var user = token.User;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            token.IsUsed = true;

            await _tokenRepository.SaveChangesAsync();
        }

    }
}