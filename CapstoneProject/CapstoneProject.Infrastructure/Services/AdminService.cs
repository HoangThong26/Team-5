using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Application.Interface.IService;
using CapstoneProject.Domain.Entities;
using ClosedXML.Excel;
using ExcelDataReader;
using System.Data;

namespace CapstoneProject.Infrastructure.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IWeeklyReportRepository _weeklyReportRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly ITopicRepository _topicRepository;
        private readonly IDefenseRepository _defenseRepository;

        public AdminService(IUserRepository userRepository, IWeeklyReportRepository weeklyReportRepository, 
            IGroupRepository groupRepository, ITopicRepository topicRepository, IDefenseRepository defenseRepository)
        {
            _userRepository = userRepository;
            _weeklyReportRepository = weeklyReportRepository;
            _groupRepository = groupRepository;
            _topicRepository = topicRepository;
            _defenseRepository = defenseRepository;
        }

        public async Task<string> CreateUserByRoleAsync(AdminCreateUserRequest request)
        {
            var existing = await _userRepository.GetByEmailAsync(request.Email);
            if (existing != null) throw new Exception("Email Exited.");
            var newUser = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                Phone = request.Phone,
                Role = request.Role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Status = "Active",
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(newUser);

            return $"Created.";
        }

        public async Task<List<User>> GetAllUsersAsync(int currentUserId)
        {
            var allUsers = await _userRepository.GetAllUsersAsync();
            return allUsers.Where(user => user.UserId != currentUserId).ToList();
        }

        public async Task DeleteAsync(int userId)
        {
            await _userRepository.DeleteAsync(userId);
        }

        public async Task UnlockAccountAsync(int userId)
        {
            await _userRepository.ChangeStatusAsync(userId);
        }
        public async Task<List<AdminUserResponse>> SearchUsersAsync(string keyword, int currentUserId)
        {
            var users = await _userRepository.SearchUsersAsync(keyword);

            return users
                .Where(u => u.UserId != currentUserId)
                .Select(u => new AdminUserResponse
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    Status = u.Status,
                    Role = u.Role
                }).ToList();
        }


        public async Task<int> ImportUsersFromExcelAsync(Stream excelStream)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var usersToImport = new List<User>();
            var existingEmails = await _userRepository.GetAllEmailsAsync();
            var existingEmailSet = new HashSet<string>(existingEmails, StringComparer.OrdinalIgnoreCase);

            var validRoles = new HashSet<string> { "Student", "Mentor", "Council", "Admin" };

            using (var reader = ExcelReaderFactory.CreateReader(excelStream))
            {
                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                });

                DataTable dt = result.Tables[0];

                foreach (DataRow row in dt.Rows)
                {
                    string email = row["Email"]?.ToString()?.Trim();
                    string fullName = row["FullName"]?.ToString()?.Trim();
                    string phone = row["Phone"]?.ToString()?.Trim();
                    string roleInput = dt.Columns.Contains("Role") ? row["Role"]?.ToString()?.Trim() : "Student";

                    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(phone))
                        continue;

                    if (existingEmailSet.Contains(email))
                        continue;

                    string finalRole = validRoles.Contains(roleInput) ? roleInput : "Student";

                    string emailPrefix = email.Contains("@") ? email.Split('@')[0] : email;
                    string phoneSuffix = phone.Length >= 3 ? phone.Substring(phone.Length - 3) : phone;
                    string rawPassword = $"{emailPrefix}{phoneSuffix}@";
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(rawPassword);

                    var newUser = new User
                    {
                        Email = email,
                        FullName = fullName,
                        Phone = phone,
                        Role = finalRole,
                        PasswordHash = hashedPassword,
                        Status = "Active",
                        EmailVerified = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    usersToImport.Add(newUser);
                    existingEmailSet.Add(email);
                }
            }
            if (usersToImport.Any())
            {
                await _userRepository.AddRangeAsync(usersToImport);
            }

            return usersToImport.Count;
        }
        public async Task<byte[]> ExportStudentsToExcelAsync()
        {
            // Lấy danh sách student (Repository của bạn đã có Include như trên)
            var students = await _userRepository.GetStudentsAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Student List");

                // 1. Header
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "FullName";
                worksheet.Cell(1, 3).Value = "Email";
                worksheet.Cell(1, 4).Value = "Phone Number";
                worksheet.Cell(1, 5).Value = "Group Name";
                worksheet.Cell(1, 6).Value = "Final Grade";

                // Định dạng Header cho đẹp
                var headerRow = worksheet.Range("A1:F1");
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

                int currentRow = 2;
                foreach (var student in students)
                {
                    worksheet.Cell(currentRow, 1).Value = student.UserId;
                    worksheet.Cell(currentRow, 2).Value = student.FullName;
                    worksheet.Cell(currentRow, 3).Value = student.Email;
                    worksheet.Cell(currentRow, 4).Value = $"'{student.Phone}";

                    var groupMember = student.GroupMember;

                    if (groupMember != null && groupMember.Group != null)
                    {
                        worksheet.Cell(currentRow, 5).Value = groupMember.Group.GroupName;

                        var finalGrade = groupMember.Group.FinalGrade;
                        if (finalGrade != null)
                        {
                            worksheet.Cell(currentRow, 6).Value = finalGrade.AverageScore;
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 6).Value = "N/A";
                        }
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 5).Value = "N/A";
                        worksheet.Cell(currentRow, 6).Value = "N/A";
                    }

                    currentRow++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
        public async Task SetupTimelineAsync(DateTime startDate)
        {
            var startDateOnly = DateOnly.FromDateTime(startDate);

            await _weeklyReportRepository.UpdateProjectStartDateAsync(startDateOnly);
            await _weeklyReportRepository.SaveChangesAsync();
        }

        public async Task<AdminDashboardStatsDto> GetDashboardStatsAsync()
        {
            var stats = new AdminDashboardStatsDto();

            // Get all groups with details
            var allGroups = await _groupRepository.GetAllGroupsWithDetailsAsync();

            // Core KPI Cards
            stats.TotalGroups = allGroups.Count;

            // Count students (users with role Student in groups)
            var studentIds = allGroups
                .SelectMany(g => g.GroupMembers)
                .Select(m => m.UserId)
                .Distinct()
                .ToList();
            stats.TotalStudents = studentIds.Count;

            // Count mentors (users assigned as mentors)
            var mentorIds = allGroups
                .Where(g => g.MentorAssignment != null)
                .Select(g => g.MentorAssignment.MentorId)
                .Distinct()
                .ToList();
            stats.TotalMentors = mentorIds.Count;

            // Topic approval stats
            var topics = new List<Topic>();
            foreach (var group in allGroups)
            {
                var topic = await _topicRepository.GetByGroupIdAsync(group.GroupId);
                if (topic != null)
                {
                    topics.Add(topic);
                }
            }
            var approvedTopics = topics.Count(t => t.Status == "Approved");
            stats.TopicApprovalStats = new TopicApprovalStatsDto
            {
                ApprovedTopics = approvedTopics,
                TotalTopics = topics.Count
            };

            // Actionable Insights & Alerts
            stats.OrphanGroupsCount = allGroups.Count(g => g.MentorAssignment == null);
            stats.PendingTopicsCount = topics.Count(t => t.Status == "Pending");

            // Report health (simplified - groups without reports this week)
            var currentWeek = await GetCurrentWeekAsync();
            var groupsNotSubmitted = new List<string>();
            var mentorsNotGraded = new List<string>();

            foreach (var group in allGroups)
            {
                var report = await _weeklyReportRepository.GetReportByGroupAndWeekAsync(group.GroupId, currentWeek);
                if (report == null)
                {
                    groupsNotSubmitted.Add(group.GroupName);
                }
                else if (report.Status != "Reviewed" && report.Status != "Pass" && report.Status != "Fail")
                {
                    mentorsNotGraded.Add(group.GroupName);
                }
            }

            stats.ReportHealthStats = new ReportHealthStatsDto
            {
                GroupsNotSubmittedThisWeek = groupsNotSubmitted.Count,
                MentorsNotGradedThisWeek = mentorsNotGraded.Count,
                GroupsNotSubmitted = groupsNotSubmitted,
                MentorsNotGraded = mentorsNotGraded
            };

            // --- US-29: CALCULATE PASS/FAIL STATS ---
            int passedCount = 0;
            int failedCount = 0;

            foreach (var group in allGroups)
            {
                // Chỉ tính các nhóm đã có điểm FinalGrade
                if (group.FinalGrade != null)
                {
                    // Giả sử điểm >= 5.0 là Pass. Bạn có thể tự đổi số 5.0m theo rule của trường
                    if (group.FinalGrade.AverageScore >= 5.0m)
                    {
                        passedCount++;
                    }
                    else
                    {
                        failedCount++;
                    }
                }
            }

            stats.PassFailStats = new PassFailStatsDto
            {
                Passed = passedCount,
                Failed = failedCount
            };

            return stats;
        }

        private async Task<int> GetCurrentWeekAsync()
        {
            var startDate = await _weeklyReportRepository.GetStartDateOfFirstWeekAsync();
            if (startDate == null) return 1;

            var now = DateTime.Now;
            var startDateTime = startDate.Value.ToDateTime(TimeOnly.MinValue);
            var daysDiff = (now - startDateTime).TotalDays;
            return Math.Max(1, (int)Math.Floor(daysDiff / 7) + 1);
        }
    }
}


