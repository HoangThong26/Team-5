using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.DTO
{
    public class AdminCreateUserRequest
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; } 
        public string Password { get; set; }
    }
}
