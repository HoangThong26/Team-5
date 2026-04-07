using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Infrastructure.Database.AppDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Infrastructure.Repostitory
{
    public class WeeklyReportRepository : IWeeklyReportRepository
    {
        private readonly ApplicationDbContext _context;
        public WeeklyReportRepository(ApplicationDbContext context) => _context = context;

        public async Task<WeeklyReport> GetReportByGroupAndWeekAsync(int groupId, int weekId) =>
            await _context.WeeklyReports.FirstOrDefaultAsync(r => r.GroupId == groupId && r.WeekId == weekId);

        public async Task AddReportAsync(WeeklyReport report) => await _context.WeeklyReports.AddAsync(report);

        public async Task<bool> IsLeaderAsync(int groupId, int userId) =>
            await _context.Groups.AnyAsync(g => g.GroupId == groupId && g.LeaderId == userId);

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task<DateOnly?> GetStartDateOfFirstWeekAsync()
        {
            var firstWeek = await _context.WeekDefinitions
                .Where(w => w.WeekNumber == 1)
                .FirstOrDefaultAsync();

            return firstWeek?.StartDate;
        }

        public async Task UpdateProjectStartDateAsync(DateOnly startDate)
        {

            var oldReports = await _context.WeeklyReports.ToListAsync();
            if (oldReports.Any()) _context.WeeklyReports.RemoveRange(oldReports);

            var oldWeeks = await _context.WeekDefinitions.ToListAsync();
            if (oldWeeks.Any()) _context.WeekDefinitions.RemoveRange(oldWeeks);

            await _context.SaveChangesAsync();
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('WeekDefinitions', RESEED, 0)");


            for (int i = 1; i <= 15; i++)
            {
                DateOnly weekStart = startDate.AddDays((i - 1) * 7);
                DateOnly weekEnd = weekStart.AddDays(6);

                var week = new WeekDefinition
                {
                    WeekNumber = i,
                    StartDate = weekStart,
                    EndDate = weekEnd
                };
                await _context.WeekDefinitions.AddAsync(week);
            }

            await _context.SaveChangesAsync();
        }
        public async Task<WeeklyReport?> GetReportByIdAsync(int reportId)
        {
            return await _context.WeeklyReports.FindAsync(reportId);
        }

        public async Task<IEnumerable<WeeklyReport>> GetReportsForMentorAsync(int mentorId, int? weekId = null, int? groupId = null, string? status = null)
        {
            var query = _context.WeeklyReports
                .Include(r => r.Group)
                .Where(r => _context.MentorAssignments
                    .Any(ma => ma.MentorId == mentorId && ma.GroupId == r.GroupId));

            if (weekId.HasValue)
            {
                query = query.Where(r => r.WeekId == weekId.Value);
            }

            if (groupId.HasValue)
            {
                query = query.Where(r => r.GroupId == groupId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }

            return await query
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();
        }

        public async Task<int?> GetMentorIdByGroupIdAsync(int groupId)
        {
            return await _context.MentorAssignments
                .Where(ma => ma.GroupId == groupId)
                .Select(ma => (int?)ma.MentorId)
                .FirstOrDefaultAsync();
        }

        public async Task<string> GetGroupNameAsync(int groupId)
        {
            return await _context.Groups
                .Where(g => g.GroupId == groupId)
                .Select(g => g.GroupName)
                .FirstOrDefaultAsync() ?? "Unknown Group";
        }

        public async Task<IEnumerable<WeeklyReport>> GetReportsByGroupIdAsync(int groupId)
        {
            return await _context.WeeklyReports
                .Where(r => r.GroupId == groupId)
                .OrderByDescending(r => r.WeekId) // Tuần mới nhất lên đầu (để [0] ở FE hoạt động đúng)
                .ThenByDescending(r => r.SubmittedAt)
                .ToListAsync();
        }

        public async Task<List<WeeklyReportHistoryDto>> GetGroupHistoryAsync(int groupId)
        {
            return await _context.WeeklyReports
                .Where(r => r.GroupId == groupId)
                .Include(r => r.WeeklyEvaluations) 
                .OrderByDescending(r => r.WeekId)    
                .Select(r => new WeeklyReportHistoryDto
                {
                    ReportId = r.ReportId,
                    WeekId = r.WeekId,
                    Content = r.Content,
                    Status = r.Status,
                    SubmittedAt = r.SubmittedAt,
                    GithubLink = r.GithubLink,
                    FileUrl = r.FileUrl,

                    MentorComment = r.WeeklyEvaluations
                        .OrderByDescending(e => e.Id)
                        .Select(e => e.Comment)
                        .FirstOrDefault(),

                    Score = r.WeeklyEvaluations
                        .OrderByDescending(e => e.Id)
                        .Select(e => e.Score)
                        .FirstOrDefault(),

                    IsPass = r.WeeklyEvaluations
                        .OrderByDescending(e => e.Id)
                        .Select(e => e.IsPass)
                        .FirstOrDefault() ?? false,

                    ReviewedAt = r.WeeklyEvaluations
                        .OrderByDescending(e => e.Id)
                        .Select(e => e.ReviewedAt)
                        .FirstOrDefault(),

                    MentorName = r.WeeklyEvaluations
                .OrderByDescending(e => e.Id)
                .Select(e => e.Mentor != null ? e.Mentor.FullName : "N/A") 
                .FirstOrDefault()
                })
                .ToListAsync();
        }


        public async Task<WeeklyReport?> GetByIdAsync(int id)
        {
            return await _context.WeeklyReports.FindAsync(id);
        }

        public async Task<bool> UpdateAsync(WeeklyReport report)
        {
            _context.WeeklyReports.Update(report);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<WeekDefinition?> GetWeekDefinitionByNumberAsync(int weekNumber)
        {
            return await _context.WeekDefinitions
                .FirstOrDefaultAsync(w => w.WeekNumber == weekNumber);
        }

    }
}
