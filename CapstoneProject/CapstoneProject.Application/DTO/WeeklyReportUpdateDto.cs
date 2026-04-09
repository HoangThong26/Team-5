using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CapstoneProject.Application.DTO
{
    public class WeeklyReportUpdateDto
    {
        [Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; } = string.Empty;

        public string? GithubLink { get; set; }

        public string? FileUrl { get; set; }
    }
}
