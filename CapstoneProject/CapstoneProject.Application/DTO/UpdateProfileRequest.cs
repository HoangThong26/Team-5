using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CapstoneProject.Application.DTO
{
    public class UpdateProfileRequest
    {

        [StringLength(500)]
        public string PasswordHash { get; set; } = null!;

        [StringLength(255)]
        public string FullName { get; set; } = null!;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(500)]
        public string? AvatarUrl { get; set; }
    }
}
