using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Application.Interface.IService;
using CapstoneProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

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

        public async Task EvaluateAsync(int mentorId, EvaluationRequest request)
        {
            var report = await _reportRepo.GetReportByIdAsync(request.ReportId);
            if (report == null) throw new Exception("Report not found.");

            if (report.Status == "Reviewed")
            {
                throw new Exception("This report has already been evaluated and is now locked.");
            }

            decimal finalScore = (decimal)request.Score;

            var evaluation = new WeeklyEvaluation
            {
                ReportId = request.ReportId,
                MentorId = mentorId,
                Score = finalScore,
                Comment = request.Comment ?? "",
                IsPass = finalScore >= 5,
                ReviewedAt = DateTime.Now
            };

            await _evaluationRepo.AddAsync(evaluation);
            report.Status = "Reviewed";

            await _evaluationRepo.SaveChangesAsync();
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
    }
}
