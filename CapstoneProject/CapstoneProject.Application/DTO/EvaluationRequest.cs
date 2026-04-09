using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.DTO
{
    public class EvaluationRequest
    {
        public int ReportId { get; set; }  // ID của báo cáo cần chấm
        public double Score { get; set; }  // Điểm số (0 - 10)
        public string? Comment { get; set; } // Nhận xét của Mentor
    }
}
