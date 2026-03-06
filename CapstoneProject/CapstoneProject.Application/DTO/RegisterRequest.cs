using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.DTO
{
    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }

    }
}
