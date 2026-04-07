using CapstoneProject.Application.DTO;
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
            var weeklyScores = await _context.WeeklyEvaluations
                .Include(e => e.Report)
                .Where(e => e.Report.GroupId == groupId)
                .Select(e => e.Score)
                .ToListAsync();

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
                    PublishedAt = null
                });
            }
            else
            {
                finalGradeEntry.AverageScore = finalScore;
                finalGradeEntry.GradeLetter = gradeLetter;
                finalGradeEntry.IsPublished = false;
                finalGradeEntry.PublishedAt = null;
            }

            await _context.SaveChangesAsync();
            return finalScore;
        }

        public async Task<List<FinalGradeResponseDto>> GetAllGradesAsync()
        {
            var result = await _context.FinalGrades
                .Include(f => f.Group)
                    .ThenInclude(g => g.Topic)
                .OrderBy(f => f.GroupId)
                .Select(f => new FinalGradeResponseDto
                {
                    GroupId = f.GroupId ?? 0,
                    AverageScore = f.AverageScore,
                    GradeLetter = f.GradeLetter,
                    IsPublished = f.IsPublished ?? false,
                    PublishedAt = f.PublishedAt,
                    Group = f.Group == null ? null : new FinalGradeGroupDto
                    {
                        GroupName = f.Group.GroupName,
                        Topic = f.Group.Topic == null ? null : new FinalGradeTopicDto
                        {
                            TopicName = f.Group.Topic.Title
                        }
                    }
                })
                .ToListAsync();

            return result;
        }

        public async Task PublishGradeAsync(int groupId)
        {
            var finalGrade = await _context.FinalGrades
                .FirstOrDefaultAsync(f => f.GroupId == groupId);

            if (finalGrade == null)
            {
                throw new Exception("Final grade not found.");
            }

            finalGrade.IsPublished = true;
            finalGrade.PublishedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }
}
