using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.DTO
{
    public class ChangePasswordWithOtpRequest
    {
        public string Otp { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
