using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Infrastructure.Database.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Infrastructure.Repostitory
{
    public class WeeklyEvaluationRepository : IWeeklyEvaluationRepository
    {
        private readonly ApplicationDbContext _context;
        public WeeklyEvaluationRepository(ApplicationDbContext context) => _context = context;

        public async Task AddAsync(WeeklyEvaluation evaluation)
            => await _context.WeeklyEvaluations.AddAsync(evaluation);

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
        public async Task<WeeklyEvaluation?> GetByReportIdAsync(int reportId)
        {
            return await _context.WeeklyEvaluations
                .Where(e => e.ReportId == reportId)
                .OrderByDescending(e => e.ReviewedAt) // Lấy bản mới nhất nếu có nhiều lần chấm
                .FirstOrDefaultAsync();
        }


        public async Task<double> GetPassCountByReportId(int reportId)
        {
            var passCount = await _context.WeeklyEvaluations
                  .Where(e => e.ReportId == reportId && e.IsPass == true)
                  .CountAsync();
            return passCount;
        }
    }
}
