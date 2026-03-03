using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }

        [HttpGet("verify")]
        public async Task<IActionResult> Verify(string token)
        {
            var result = await _authService.VerifyAsync(token);
            return Ok(result);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var result = await _authService.LoginAsync(request,ipAddress);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request.RefreshToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            var result = await _authService.LogoutAsync(request.RefreshToken);

            if (result)
                return Ok(new { message = "Logout." });

            return BadRequest("Token incorrect.");
        }

        public class LogoutRequest
        {
            public string RefreshToken { get; set; }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequestDto request)
        {
            try
            {
                await _authService.ForgotPasswordAsync(request.Email);
                return Ok(new
                {
                    message = "OTP Sending."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordRequestDto request)
        {
            try
            {
                await _authService.ResetPasswordAsync(request);

                return Ok(new
                {
                    message = "Password change success"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

    }
}
