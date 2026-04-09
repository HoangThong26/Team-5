using CapstoneProject.Application.DTO;

namespace CapstoneProject.Application.Interface.IService
{
    public interface IWeeklyEvaluationService
    {
        Task<string> EvaluateAsync(int mentorId, EvaluationRequest request);
        Task<EvaluationResponseDTO?> GetEvaluationByReportIdAsync(int reportId);
        Task<double> CaculateGoToCoulcing(int reportId);
        Task<List<WeeklyReportSectionDTO>> GetPendingReportsAsync();
    }
}
