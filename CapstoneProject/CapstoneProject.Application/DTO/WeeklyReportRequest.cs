using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.DTO
{
    public class WeeklyReportRequest
    {
        public int GroupId { get; set; }
        public int WeekId { get; set; }
        public string Content { get; set; }
        public string? GithubLink { get; set; }
        public IFormFile? ReportFile { get; set; }
    }
}
