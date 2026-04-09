using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Interface.IRepository
{
    public interface IWeeklyEvaluationRepository
    {
        Task AddAsync(WeeklyEvaluation evaluation);
        Task<WeeklyEvaluation?> GetByReportIdAsync(int reportId);
        Task SaveChangesAsync();
        Task<Double> GetPassCountByReportId(int reportId);
    }
}
