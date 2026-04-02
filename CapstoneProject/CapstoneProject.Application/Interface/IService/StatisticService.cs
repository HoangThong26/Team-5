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

        // Thêm hàm này vào bên trong class StatisticService (dưới hàm GetPassFailStatisticsAsync cũ)
        public async Task<List<GradeDistributionDto>> GetGradeDistributionAsync(int? semesterId, int? majorId)
        {
            // 1. Khởi tạo Query: Chỉ lấy những nhóm đã có điểm (FinalGrade != null) và có điểm chữ
            var query = _context.Groups
                .Include(g => g.FinalGrade)
                .Where(g => g.FinalGrade != null && !string.IsNullOrEmpty(g.FinalGrade.GradeLetter))
                .AsQueryable();

            // 2. Áp dụng bộ lọc Semester và Major
            if (semesterId.HasValue)
            {
                query = query.Where(g => g.SemesterId == semesterId.Value);
            }

            if (majorId.HasValue)
            {
                query = query.Where(g => g.MajorId == majorId.Value);
            }

            // 3. Gom nhóm theo GradeLetter và đếm
            var result = await query
                .GroupBy(g => g.FinalGrade!.GradeLetter)
                .Select(group => new GradeDistributionDto
                {
                    GradeLetter = group.Key!,
                    Count = group.Count()
                })
                .OrderBy(x => x.GradeLetter) // Sắp xếp theo thứ tự chữ cái (A, B, C...)
                .ToListAsync();

            return result;
        }
    }
}