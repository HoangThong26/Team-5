using CapstoneProject.Application.DTO;
using CapstoneProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.Interface.IRepository
{
    public interface IWeeklyReportRepository
    {
        Task<WeeklyReport> GetReportByGroupAndWeekAsync(int groupId, int weekId);
        Task AddReportAsync(WeeklyReport report);
        Task<bool> IsLeaderAsync(int groupId, int userId);
        Task SaveChangesAsync();
        Task<DateOnly?> GetStartDateOfFirstWeekAsync();
        Task<WeeklyReport?> GetReportByIdAsync(int reportId);
        Task UpdateProjectStartDateAsync(DateOnly startDate);
        Task<IEnumerable<WeeklyReport>> GetReportsForMentorAsync(int mentorId, int? weekId = null, int? groupId = null, string? status = null);
        Task<int?> GetMentorIdByGroupIdAsync(int groupId);
        Task<string> GetGroupNameAsync(int groupId);
        Task<IEnumerable<WeeklyReport>> GetReportsByGroupIdAsync(int groupId);
        Task<List<WeeklyReportHistoryDto>> GetGroupHistoryAsync(int groupId);
        Task<WeeklyReport?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(WeeklyReport report);
        Task<WeekDefinition?> GetWeekDefinitionByNumberAsync(int weekNumber);

   


    }
}
