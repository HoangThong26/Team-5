using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Application.Interface.IService;
using Microsoft.Extensions.Caching.Memory;

namespace CapstoneProject.Infrastructure.Services
{
    public class EvaluationService : IEvaluationService
    {
        private readonly IWeeklyReportRepository _reportRepo;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;

        public EvaluationService(IWeeklyReportRepository reportRepo, IEmailService emailService, IMemoryCache cache)
        {
            _reportRepo = reportRepo;
            _emailService = emailService;
            _cache = cache;
        }

        public async Task CheckAndSendRemindersAsync()
        {
            var reports = await _reportRepo.GetReportsForReminderAsync();

            foreach (var report in reports)
            {
                var mentorEmail = report.Group?.MentorAssignment?.Mentor?.Email;
                if (string.IsNullOrEmpty(mentorEmail)) continue;

                string cacheKey = $"Reminder_Sent_Report_{report.ReportId}";

                if (!_cache.TryGetValue(cacheKey, out bool isSent))
                {
                    // Email Content in English
                    string subject = "[URGENT] Weekly Report Evaluation Deadline Approaching";
                    string body = $@"
                <h3>Evaluation Reminder</h3>
                <p>Dear Mentor,</p>
                <p>The weekly report from Group: <b>{report.Group.GroupName}</b> is reaching its deadline.</p>
                <p>You have <b>less than 2 hours</b> remaining to complete the evaluation before it is marked as <b>LATE</b>.</p>
                <p>Please log in to the system and submit your evaluation as soon as possible.</p>
                <br>
                <p>Best regards,</p>
                <p><b>Capstone Management System</b></p>";

                    await _emailService.SendEmailAsync(mentorEmail, subject, body);

                    // Store in cache for 3 hours to prevent duplicate emails for the same report
                    _cache.Set(cacheKey, true, TimeSpan.FromHours(3));
                }
            }
        }
    }
}

