namespace CapstoneProject.Application.DTO
{
    public class WeeklyReportSectionDTO
    {
        public int ReportId { get; set; }
        public DateTime? SubmittedAt { get; set; }

        public string RemainingTime { get; set; }

        public bool IsExpired { get; set; }
    }
}
