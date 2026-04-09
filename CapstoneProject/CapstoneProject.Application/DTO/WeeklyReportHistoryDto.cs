using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.DTO
{
    public class WeeklyReportHistoryDto
    {
        public int ReportId { get; set; }
        public int? WeekId { get; set; }
        public string? Content { get; set; }
        public string? Status { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? GithubLink { get; set; }
        public string? FileUrl { get; set; }
        public string? MentorComment { get; set; }

        public decimal? Score { get; set; }

        public bool IsPass { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? MentorName { get; set; }
    }
}
