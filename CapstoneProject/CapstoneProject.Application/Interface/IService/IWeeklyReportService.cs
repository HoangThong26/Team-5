using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.Interface.IService
{
    public interface IWeeklyReportService
    {
        Task<ServiceResponse<WeeklyReport>> SubmitReportAsync(int userId, WeeklyReportRequest request);
        Task<IEnumerable<WeeklyReport>> GetReportsForMentorAsync(int mentorId);
        Task<IEnumerable<WeeklyReport>> GetReportsByGroupIdAsync(int groupId);
        Task<List<WeeklyReportHistoryDto>> GetGroupHistoryAsync(int groupId);
        Task<ServiceResponse<WeeklyReport>> UpdateWeeklyReportAsync(int reportId, WeeklyReportRequest request);
        Task<(byte[] fileContent, string contentType, string fileName)> DownloadReportAsync(string fileName);
    }
}
