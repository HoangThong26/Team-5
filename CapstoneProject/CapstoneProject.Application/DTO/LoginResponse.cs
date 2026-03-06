using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.DTO
{
    public class LoginResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public TokenResponse? Tokens { get; set; }
    }
}
