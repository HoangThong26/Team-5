using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Application.Interface.IService;
using CapstoneProject.Domain.Entities;
using Microsoft.AspNetCore.Hosting;

namespace CapstoneProject.Infrastructure.Services
{
    public class WeeklyReportService : IWeeklyReportService
    {
        private readonly IWeeklyReportRepository _weeklyReportRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly ITopicRepository _topicRepository;
        public WeeklyReportService(IWeeklyReportRepository weeklyReportRepository, IWebHostEnvironment environment, ITopicRepository topicRepository)
        {
            _weeklyReportRepository = weeklyReportRepository;
            _environment = environment;
            _topicRepository = topicRepository;
        }


        public async Task<ServiceResponse<WeeklyReport>> SubmitReportAsync(int userId, WeeklyReportRequest request)
        {
            var response = new ServiceResponse<WeeklyReport>();

            try
            {
                var isTopicApproved = await _topicRepository.IsTopicApprovedForGroupAsync(request.GroupId);
                if (!isTopicApproved)
                {
                    throw new Exception("Forbidden: Your group topic is either not registered or not 'Approved' by Mentor. You cannot submit weekly reports yet.");
                }
                var startDateOnly = await _weeklyReportRepository.GetStartDateOfFirstWeekAsync();
                if (startDateOnly == null)
                    throw new Exception("System Error: Project timeline has not been configured by Admin.");

                DateTime startDate = startDateOnly.Value.ToDateTime(TimeOnly.MinValue);
                DateTime now = DateTime.Now;
                int currentSystemWeek = (int)Math.Floor((now - startDate).TotalDays / 7) + 1;

                var targetWeek = await _weeklyReportRepository.GetWeekDefinitionByNumberAsync(request.WeekId);
                if (targetWeek == null)
                    throw new Exception($"Error: Week {request.WeekId} is not valid.");
                if (!targetWeek.EndDate.HasValue)
                    throw new Exception($"System Error: Week {request.WeekId} timeline is missing end date.");
                if (request.WeekId > currentSystemWeek)
                    throw new Exception($"Forbidden: You cannot submit for Week {request.WeekId} yet. Current progress is Week {currentSystemWeek}.");
                DateTime deadline = targetWeek.EndDate.Value.ToDateTime(new TimeOnly(23, 59, 59));
                if (now > deadline)
                    throw new Exception($"Overdue: Deadline for Week {request.WeekId} was {targetWeek.EndDate.Value:dd/MM/yyyy}. Submission closed.");
                var isLeader = await _weeklyReportRepository.IsLeaderAsync(request.GroupId, userId);
                if (!isLeader)
                    throw new Exception("Access Denied: Only the Group Leader can submit reports.");

                string? savedFileName = null;
                if (request.ReportFile != null && request.ReportFile.Length > 0)
                {
                    var extension = Path.GetExtension(request.ReportFile.FileName).ToLower();
                    if (extension != ".doc" && extension != ".docx")
                        throw new Exception("Invalid file type. Only Word documents (.doc, .docx) are allowed.");

                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UploadedReports");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    savedFileName = $"Group_{request.GroupId}_Week_{request.WeekId}_{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsFolder, savedFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.ReportFile.CopyToAsync(fileStream);
                    }
                }

                var existingReport = await _weeklyReportRepository.GetReportByGroupAndWeekAsync(request.GroupId, request.WeekId);

                if (existingReport == null)
                {
                    existingReport = new WeeklyReport
                    {
                        GroupId = request.GroupId,
                        WeekId = request.WeekId,
                        Content = request.Content,
                        GithubLink = request.GithubLink,
                        FileUrl = savedFileName,
                        Status = "Submitted",
                        SubmittedAt = now
                    };
                    await _weeklyReportRepository.AddReportAsync(existingReport);
                    response.Message = $"Report for Week {request.WeekId} created successfully.";
                }
                else
                {
                    if (IsEvaluatedStatus(existingReport.Status))
                        throw new Exception("Locked: Report is already reviewed and cannot be edited.");

                    existingReport.Content = request.Content;
                    existingReport.GithubLink = request.GithubLink;
                    if (!string.IsNullOrEmpty(savedFileName)) existingReport.FileUrl = savedFileName;
                    existingReport.SubmittedAt = now;
                    existingReport.Status = "Submitted";
                    response.Message = $"Report for Week {request.WeekId} updated successfully.";
                }

                await _weeklyReportRepository.SaveChangesAsync();
                response.Data = existingReport;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<IEnumerable<WeeklyReport>> GetReportsForMentorAsync(int mentorId)
        {
            return await _weeklyReportRepository.GetReportsForMentorAsync(mentorId);
        }
        public async Task<IEnumerable<WeeklyReport>> GetReportsByGroupIdAsync(int groupId)
        {
            return await _weeklyReportRepository.GetReportsByGroupIdAsync(groupId);
        }
        public async Task<List<WeeklyReportHistoryDto>> GetGroupHistoryAsync(int groupId)
        {
            if (groupId <= 0)
            {
                return new List<WeeklyReportHistoryDto>();
            }

            var history = await _weeklyReportRepository.GetGroupHistoryAsync(groupId);

            return history;
        }

        public async Task<CouncilEligibilityDto> GetCouncilEligibilityAsync(int groupId)
        {
            const int totalWeeks = 15;
            const decimal maxScorePerWeek = 10m;
            decimal maxScore = totalWeeks * maxScorePerWeek;

            if (groupId <= 0)
            {
                return new CouncilEligibilityDto
                {
                    GroupId = groupId,
                    TotalWeeks = totalWeeks,
                    MaxScore = maxScore,
                    TotalScore = 0,
                    Percentage = 0,
                    IsEligibleForCouncil = false,
                    EvaluatedWeeks = 0
                };
            }

            var history = await _weeklyReportRepository.GetGroupHistoryAsync(groupId);
            var historyInRange = history
                .Where(h => (h.WeekId ?? 0) >= 1 && (h.WeekId ?? 0) <= totalWeeks)
                .ToList();

            decimal totalScore = historyInRange.Sum(h => h.Score ?? 0m);
            int evaluatedWeeks = historyInRange.Count(h => h.Score.HasValue);
            double percentage = maxScore == 0 ? 0 : Math.Round((double)(totalScore / maxScore * 100m), 2);

            return new CouncilEligibilityDto
            {
                GroupId = groupId,
                TotalWeeks = totalWeeks,
                MaxScore = maxScore,
                TotalScore = totalScore,
                Percentage = percentage,
                IsEligibleForCouncil = percentage > 80,
                EvaluatedWeeks = evaluatedWeeks
            };
        }

        public async Task<ServiceResponse<WeeklyReport>> UpdateWeeklyReportAsync(int reportId, WeeklyReportRequest request)
        {
            var response = new ServiceResponse<WeeklyReport>();

            try
            {
                var existingReport = await _weeklyReportRepository.GetByIdAsync(reportId);
                if (existingReport == null)
                {
                    response.Success = false;
                    response.Message = "Weekly report not found.";
                    return response;
                }

                var targetWeek = await _weeklyReportRepository.GetWeekDefinitionByNumberAsync(existingReport.WeekId ?? 0);
                if (targetWeek == null || !targetWeek.EndDate.HasValue)
                {
                    throw new Exception("System Error: Week timeline is not configured properly.");
                }
                DateTime now = DateTime.Now;
                DateTime deadline = targetWeek.EndDate.Value.ToDateTime(new TimeOnly(23, 59, 59));

                if (now > deadline)
                {
                    response.Success = false;
                    response.Message = $"Update Failed: The deadline for Week {existingReport.WeekId} has passed ({targetWeek.EndDate.Value:dd/MM/yyyy}). Submission portal is closed.";
                    return response;
                }

                if (IsEvaluatedStatus(existingReport.Status))
                {
                    response.Success = false;
                    response.Message = "Cannot edit: Mentor has already reviewed this report.";
                    return response;
                }

                if (request.ReportFile != null && request.ReportFile.Length > 0)
                {
                    var extension = Path.GetExtension(request.ReportFile.FileName).ToLower();
                    if (extension != ".doc" && extension != ".docx")
                        throw new Exception("Invalid file type. Only .doc and .docx are allowed.");

                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UploadedReports");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var fileName = $"Update_Grp{existingReport.GroupId}_W{existingReport.WeekId}_{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.ReportFile.CopyToAsync(fileStream);
                    }

                    existingReport.FileUrl = fileName;
                }

                existingReport.Content = request.Content;
                existingReport.GithubLink = request.GithubLink;
                existingReport.SubmittedAt = now;
                existingReport.Status = "Submitted";

                await _weeklyReportRepository.SaveChangesAsync();

                response.Data = existingReport;
                response.Success = true;
                response.Message = "Updated successfully!";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Internal Error: {ex.Message}";
            }

            return response;
        }

        public async Task<(byte[] fileContent, string contentType, string fileName)> DownloadReportAsync(string fileName)
        {
            string rootPath = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(rootPath, "UploadedReports", fileName);

            if (!System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException("This report no longer exists on the system.");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

            var contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

            return (fileBytes, contentType, fileName);
        }

        private static bool IsEvaluatedStatus(string? status)
        {
            return status?.Equals("Reviewed", StringComparison.OrdinalIgnoreCase) == true
                || status?.Equals("Pass", StringComparison.OrdinalIgnoreCase) == true
                || status?.Equals("Fail", StringComparison.OrdinalIgnoreCase) == true;
        }

    }
}
