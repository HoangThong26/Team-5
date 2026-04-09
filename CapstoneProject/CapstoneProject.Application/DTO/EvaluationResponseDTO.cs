using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.DTO
{
    public class EvaluationResponseDTO
    {
        public int? ReportId { get; set; }
        public bool? IsPass { get; set; }
        public decimal? Score { get; set; }
        public string? Comment { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? MentorName { get; set; } = string.Empty;
    }
}
