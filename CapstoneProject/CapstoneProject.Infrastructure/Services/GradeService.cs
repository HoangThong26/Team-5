using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Application.Interface.IService;
using CapstoneProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Infrastructure.Database.AppDbContext;

namespace CapstoneProject.Infrastructure.Services
{
    public class GradeService : IGradeService
    {
        private readonly IFinalGradeRepository _gradeRepository;
        private readonly ApplicationDbContext _context; // Giữ lại context nếu cần truy vấn bảng khác chưa có Repo

        public GradeService(IFinalGradeRepository gradeRepository, ApplicationDbContext context)
        {
            _gradeRepository = gradeRepository;
            _context = context;
        }

        public async Task<decimal> CalculateAndSaveFinalGrade(int groupId)
        {
            // 1. Lấy điểm Weekly (Vẫn dùng context cho các bảng chưa có Repo)
            var weeklyScores = await _context.WeeklyEvaluations
                .Include(e => e.Report)
                .Where(e => e.Report.GroupId == groupId)
                .Select(e => e.Score)
                .ToListAsync();

            // 2. Lấy điểm Defense
            var defenseScores = await _context.DefenseScores
                .Include(s => s.Defense)
                .Where(s => s.Defense.GroupId == groupId)
                .Select(s => s.Score)
                .ToListAsync();

            if (!weeklyScores.Any() || !defenseScores.Any())
            {
                throw new Exception("Insufficient data: Missing weekly evaluations or defense scores.");
            }

            decimal avgWeekly = (decimal)weeklyScores.Average();
            decimal avgDefense = (decimal)defenseScores.Average();
            decimal finalScore = Math.Round((avgWeekly * 0.4m) + (avgDefense * 0.6m), 2);

            string gradeLetter = finalScore >= 5.0m ? "PASSED" : "FAILED";

            // 3. Sử dụng Repository để lưu
            var finalGradeEntry = await _gradeRepository.GetByGroupIdAsync(groupId);

            if (finalGradeEntry == null)
            {
                await _gradeRepository.AddAsync(new FinalGrade
                {
                    GroupId = groupId,
                    AverageScore = finalScore,
                    GradeLetter = gradeLetter,
                    IsPublished = false,
                });
            }
            else
            {
                finalGradeEntry.AverageScore = finalScore;
                finalGradeEntry.GradeLetter = gradeLetter;
                _gradeRepository.Update(finalGradeEntry);
            }

            await _gradeRepository.SaveChangesAsync();
            return finalScore;
        }

        public async Task PublishGrade(int groupId)
        {
            var grade = await _gradeRepository.GetByGroupIdAsync(groupId);
            if (grade != null)
            {
                grade.IsPublished = true;
                grade.PublishedAt = DateTime.Now;
                _gradeRepository.Update(grade);
                await _gradeRepository.SaveChangesAsync();
            }
        }

        public async Task<FinalGrade?> GetGradeForStudent(int groupId)
        {
            var grade = await _gradeRepository.GetByGroupIdAsync(groupId);

            if (grade == null) return null;

            // Logic AC3: Nếu chưa công bố, không trả về điểm
            if (!grade.IsPublished.GetValueOrDefault())
            {
                return new FinalGrade
                {
                    GroupId = groupId,
                    IsPublished = false
                };
            }

            return grade;
        }

        public async Task<List<FinalGrade>> GetAllFinalGrades()
        {
            return await _gradeRepository.GetAllWithGroupsAsync();
        }
    }
}