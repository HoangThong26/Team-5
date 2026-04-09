using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Application.Interface.IService;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Infrastructure.Services
{
    public class WeeklyEvaluationService : IWeeklyEvaluationService
    {
        private readonly IWeeklyEvaluationRepository _evaluationRepo;
        private readonly IWeeklyReportRepository _reportRepo;

        public WeeklyEvaluationService(
            IWeeklyEvaluationRepository evaluationRepo,
            IWeeklyReportRepository reportRepo)
        {
            _evaluationRepo = evaluationRepo;
            _reportRepo = reportRepo;
        }

        public async Task<string> EvaluateAsync(int mentorId, EvaluationRequest request)
        {
            var report = await _reportRepo.GetReportByIdAsync(request.ReportId);
            if (report == null) throw new Exception("Report not found.");

            if (report.Status == "Reviewed")
            {
                throw new Exception("This report has already been evaluated and is now locked.");
            }

            bool isLate = false;
            string finalComment = request.Comment ?? "";

            if (report.SubmittedAt.HasValue)
            {
                DateTime deadline = report.SubmittedAt.Value.AddDays(2);

                if (DateTime.Now > deadline)
                {
                    isLate = true;
                    finalComment = $"[LATE EVALUATION] {finalComment}";
                }
            }

            decimal finalScore = (decimal)request.Score;

            var evaluation = new WeeklyEvaluation
            {
                ReportId = request.ReportId,
                MentorId = mentorId, // System logs who evaluated here
                Score = finalScore,
                Comment = finalComment,
                IsPass = finalScore >= 5,
                ReviewedAt = DateTime.Now // System logs exactly when they evaluated
            };

            await _evaluationRepo.AddAsync(evaluation);
            report.Status = "Reviewed";

            await _evaluationRepo.SaveChangesAsync();

            // Return English messages to the controller
            if (isLate)
            {
                return "Evaluation submitted successfully, but it has been flagged as LATE (over 48-hour limit).";
            }

            return "Report evaluated successfully.";
        }

        public async Task<EvaluationResponseDTO?> GetEvaluationByReportIdAsync(int reportId)
        {
            var evaluation = await _evaluationRepo.GetByReportIdAsync(reportId);
            if (evaluation == null) return null;

            return new EvaluationResponseDTO
            {
                ReportId = evaluation.ReportId,
                IsPass = evaluation.IsPass,
                Score = evaluation.Score,
                Comment = evaluation.Comment,
                ReviewedAt = evaluation.ReviewedAt,
                MentorName = evaluation.Mentor?.FullName ?? "Unknown Mentor"
            };
        }


        public async Task<double> CaculateGoToCoulcing(int reportId)
        {
            var passCount = await _evaluationRepo.GetPassCountByReportId(reportId);
            double percent = passCount / 15.0 * 100;
            return percent;
        }

        public async Task<List<WeeklyReportSectionDTO>> GetPendingReportsAsync()
        {
            var reports = await _reportRepo.GetPendingReportsAsync();
            var now = DateTime.Now;

            return reports.Select(r =>
            {
                var deadline = r.SubmittedAt.Value.AddDays(2);
                var timeSpan = deadline - now;

                bool isExpired = timeSpan.TotalSeconds <= 0;
                string remaining;

                if (isExpired)
                {
                    remaining = "Expired";
                }
                else
                {
                    remaining = $"{(int)timeSpan.TotalDays}d {timeSpan.Hours}h {timeSpan.Minutes}m";
                }

                return new WeeklyReportSectionDTO
                {
                    ReportId = r.ReportId,
                    SubmittedAt = r.SubmittedAt,
                    RemainingTime = remaining,
                    IsExpired = isExpired
                };
            }).ToList();
        }
    }
}