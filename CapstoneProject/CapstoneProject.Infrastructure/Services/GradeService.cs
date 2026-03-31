using CapstoneProject.Application.Interface.IService;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Infrastructure.Database.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Infrastructure.Services
{
    public class GradeService : IGradeService
    {
        private readonly ApplicationDbContext _context;

        public GradeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalculateAndSaveFinalGrade(int groupId)
        {
            // 1. Lấy điểm trung bình Weekly Progress (Trọng số 40%)
            var weeklyScores = await _context.WeeklyEvaluations
                .Include(e => e.Report)
                .Where(e => e.Report.GroupId == groupId)
                .Select(e => e.Score)
                .ToListAsync();

            // 2. Lấy điểm trung bình Defense từ hội đồng (Trọng số 60%)
            var defenseScores = await _context.DefenseScores
                .Include(s => s.Defense)
                .Where(s => s.Defense.GroupId == groupId)
                .Select(s => s.Score)
                .ToListAsync();

            // AC3: Kiểm tra thiếu dữ liệu
            if (!weeklyScores.Any() || !defenseScores.Any())
            {
                throw new Exception("Insufficient data: Missing weekly evaluations or defense scores.");
            }

            // 3. Áp dụng công thức tính Final Grade
            decimal avgWeekly = (decimal)weeklyScores.Average();
            decimal avgDefense = (decimal)defenseScores.Average();
            decimal finalScore = Math.Round((avgWeekly * 0.4m) + (avgDefense * 0.6m), 2);

            // 4. Xác định GradeLetter (Quy tắc mẫu)
            string gradeLetter = finalScore >= 5.0m ? "PASSED" : "FAILED";

            // 5. Lưu vào bảng FinalGrades
            var finalGradeEntry = await _context.FinalGrades
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (finalGradeEntry == null)
            {
                _context.FinalGrades.Add(new FinalGrade
                {
                    GroupId = groupId,
                    AverageScore = finalScore,
                    GradeLetter = gradeLetter,
                    IsPublished = false,
                    PublishedAt = DateTime.Now
                });
            }
            else
            {
                finalGradeEntry.AverageScore = finalScore;
                finalGradeEntry.GradeLetter = gradeLetter;
                finalGradeEntry.PublishedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return finalScore;
        }
    }
}