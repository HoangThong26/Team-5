using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.DTO
{
    public class UserViewDTO
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string AvatarUrl { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public bool EmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
