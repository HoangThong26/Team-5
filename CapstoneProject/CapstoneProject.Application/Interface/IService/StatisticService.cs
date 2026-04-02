using CapstoneProject.Application.DTOs;
using CapstoneProject.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Services
{
    public class StatisticService : IStatisticService
    {
        // Sử dụng Interface thay vì Class thực tế
        private readonly IApplicationDbContext _context;

        public StatisticService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PassFailStatisticDto> GetPassFailStatisticsAsync(int? semesterId, int? majorId)
        {
            var query = _context.Groups
                .Include(g => g.FinalGrade)
                .AsQueryable();

            if (semesterId.HasValue)
            {
                query = query.Where(g => g.SemesterId == semesterId.Value);
            }

            if (majorId.HasValue)
            {
                query = query.Where(g => g.MajorId == majorId.Value);
            }

            var result = await query
                .GroupBy(g => 1)
                .Select(group => new PassFailStatisticDto
                {
                    TotalGroups = group.Count(),
                    Pass = group.Count(g => g.FinalGrade != null && g.FinalGrade.IsPass == true),
                    Fail = group.Count(g => g.FinalGrade != null && g.FinalGrade.IsPass == false),
                    Pending = group.Count(g => g.FinalGrade == null || g.FinalGrade.IsPass == null)
                })
                .FirstOrDefaultAsync();

            return result ?? new PassFailStatisticDto();
        }
    }
}