using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Application.Interface.IService;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Infrastructure.Database.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Service
{
    public class DefenseService : IDefenseService
    {
        private readonly IDefenseRepository _defenseRepository;
        private readonly IGroupService _groupService;
        private readonly IWeeklyReportRepository _weeklyReportRepository;
        private readonly ApplicationDbContext _context;

        public DefenseService(
            ApplicationDbContext context,
            IDefenseRepository defenseRepository,
            IGroupService groupService,
            IWeeklyReportRepository weeklyReportRepository)
        {
            _context = context;
            _defenseRepository = defenseRepository;
            _groupService = groupService;
            _weeklyReportRepository = weeklyReportRepository;
        }

        public async Task<DefenseRegistrationStatusDto?> GetMyRegistrationStatusAsync(int userId)
        {
            const int totalWeeks = 15;
            const decimal maxScorePerWeek = 10m;
            var maxScore = totalWeeks * maxScorePerWeek;

            var myGroup = await _groupService.GetMyGroupAsync(userId);
            if (myGroup == null)
            {
                return null;
            }

            var history = await _weeklyReportRepository.GetGroupHistoryAsync(myGroup.GroupId);
            var historyInRange = history
                .Where(h => (h.WeekId ?? 0) >= 1 && (h.WeekId ?? 0) <= totalWeeks)
                .ToList();

            var evaluatedWeeks = historyInRange.Count(h => h.Score.HasValue);
            var passedWeeks = historyInRange.Count(h => h.Score.HasValue && h.IsPass);
            var totalScore = historyInRange.Sum(h => h.Score ?? 0m);
            var passRate = maxScore == 0
                ? 0
                : Math.Round((double)(totalScore / maxScore * 100m), 2);

            var existingRegistration = await _defenseRepository.GetDefenseByGroupIdAsync(myGroup.GroupId);

            return new DefenseRegistrationStatusDto
            {
                GroupId = myGroup.GroupId,
                GroupName = myGroup.GroupName,
                EvaluatedWeeks = evaluatedWeeks,
                PassedWeeks = passedWeeks,
                PassRate = passRate,
                IsEligibleForDefense = passRate > 80,
                IsRegistered = existingRegistration != null
            };
        }

        public async Task RegisterMyGroupAsync(int userId)
        {
            var registrationStatus = await GetMyRegistrationStatusAsync(userId);
            if (registrationStatus == null)
            {
                throw new Exception("You are not in any group.");
            }

            if (!registrationStatus.IsEligibleForDefense)
            {
                throw new Exception("Not eligible for defense");
            }

            if (registrationStatus.IsRegistered)
            {
                throw new Exception("Group has already registered for defense.");
            }

            var newRegistration = new DefenseSchedule
            {
                GroupId = registrationStatus.GroupId,
                Status = "Scheduled"
            };

            await _defenseRepository.AddDefenseScheduleAsync(newRegistration);
            await _defenseRepository.SaveChangesAsync();
        }

        public async Task<List<DefenseRegistrationItemDto>> GetDefenseRegistrationsAsync()
        {
            var registrations = await _defenseRepository.GetDefenseRegistrationsAsync();

            return registrations.Select(item => new DefenseRegistrationItemDto
            {
                DefenseId = item.DefenseId,
                GroupId = item.GroupId ?? 0,
                GroupName = item.Group?.GroupName ?? "Unknown Group",
                Status = item.Status ?? "Registered",
                CouncilId = item.CouncilId,
                CouncilName = item.Council?.Name,
                Room = item.Room,
                StartTime = item.StartTime,
                EndTime = item.EndTime
            }).ToList();
        }

        public async Task<List<DefenseCommitteeDto>> GetDefenseCommitteesAsync()
        {
            var councils = await _defenseRepository.GetAllCouncilsWithMembersAsync();

            return councils.Select(c => new DefenseCommitteeDto
            {
                CouncilId = c.CouncilId,
                CouncilName = c.Name ?? $"Council #{c.CouncilId}",
                Members = c.CouncilMembers
                    .Where(m => m.UserId.HasValue)
                    .Select(m => new DefenseCommitteeMemberDto
                    {
                        UserId = m.UserId ?? 0,
                        FullName = m.User?.FullName ?? "Unknown"
                    })
                    .ToList()
            }).ToList();
        }

        public async Task<List<CouncilUserDto>> GetCouncilUsersAsync()
        {
            var councils = await _defenseRepository.GetAllCouncilsWithMembersAsync();

            return councils
                .SelectMany(c => c.CouncilMembers
                    .Where(m => m.UserId.HasValue)
                    .Select(m => new CouncilUserDto
                    {
                        UserId = m.UserId ?? 0,
                        FullName = m.User?.FullName ?? "Unknown",
                        CouncilId = c.CouncilId,
                        CouncilName = c.Name ?? $"Council #{c.CouncilId}"
                    }))
                .ToList();
        }

        public async Task CreateDefenseScheduleAsync(CreateDefenseScheduleRequest request)
        {
            var defense = await _defenseRepository.GetDefenseByIdAsync(request.DefenseId)
                ?? throw new Exception("Defense registration not found.");

            ValidateScheduleRequest(request.CouncilId, request.Room, request.StartTime, request.EndTime);

            var council = await _defenseRepository.GetCouncilByIdAsync(request.CouncilId);
            if (council == null)
            {
                throw new Exception("Defense committee not found.");
            }

            var councilMemberCount = await _defenseRepository.GetCouncilMemberCountAsync(request.CouncilId);
            if (councilMemberCount < 3)
            {
                throw new Exception("Each defense room must have at least 3 council members.");
            }

            var hasExistingSchedule = defense.CouncilId.HasValue
                || !string.IsNullOrWhiteSpace(defense.Room)
                || defense.StartTime.HasValue;

            if (hasExistingSchedule)
            {
                throw new Exception("Defense schedule already exists. Please use update.");
            }

            defense.CouncilId = request.CouncilId;
            defense.Room = request.Room.Trim();
            defense.StartTime = request.StartTime;
            defense.EndTime = request.EndTime;
            defense.Status = "Scheduled";

            await _defenseRepository.UpdateDefenseScheduleAsync(defense);
            await _defenseRepository.SaveChangesAsync();
        }

        public async Task UpdateDefenseScheduleAsync(int defenseId, UpdateDefenseScheduleRequest request)
        {
            var defense = await _defenseRepository.GetDefenseByIdAsync(defenseId)
                ?? throw new Exception("Defense registration not found.");

            ValidateScheduleRequest(request.CouncilId, request.Room, request.StartTime, request.EndTime);

            var council = await _defenseRepository.GetCouncilByIdAsync(request.CouncilId);
            if (council == null)
            {
                throw new Exception("Defense committee not found.");
            }

            var councilMemberCount = await _defenseRepository.GetCouncilMemberCountAsync(request.CouncilId);
            if (councilMemberCount < 3)
            {
                throw new Exception("Each defense room must have at least 3 council members.");
            }

            defense.CouncilId = request.CouncilId;
            defense.Room = request.Room.Trim();
            defense.StartTime = request.StartTime;
            defense.EndTime = request.EndTime;
            defense.Status = "Scheduled";

            await _defenseRepository.UpdateDefenseScheduleAsync(defense);
            await _defenseRepository.SaveChangesAsync();
        }

        private static void ValidateScheduleRequest(int councilId, string room, DateTime startTime, DateTime? endTime)
        {
            if (councilId <= 0)
            {
                throw new Exception("Defense committee is required.");
            }

            if (string.IsNullOrWhiteSpace(room))
            {
                throw new Exception("Room is required.");
            }

            if (startTime == default)
            {
                throw new Exception("Start time is required.");
            }

            if (endTime.HasValue && endTime.Value <= startTime)
            {
                throw new Exception("End time must be after start time.");
            }
        }

        public async Task<bool> SubmitEvaluationAsync(DefenseEvaluationRequest request)
        {
            // 1. Kiểm tra giảng viên có thuộc hội đồng chấm lịch này không
            var councilMember = await _defenseRepository.GetCouncilMemberByUserAndDefenseAsync(request.CouncilMemberUserId, request.DefenseId);
            if (councilMember == null) throw new Exception("You are not authorized to evaluate this defense.");

            // 2. Tính toán lại điểm Final ở BE (đảm bảo an toàn dữ liệu)
            // Trọng số: P: 30%, D: 50%, Q: 20%
            decimal calculatedScore = (request.PresentationScore * 0.3m) +
                                     (request.DemoScore * 0.5m) +
                                     (request.QAScore * 0.2m);

            // 3. Kiểm tra xem đã có điểm chưa để Update hoặc Add mới
            var existingScore = await _defenseRepository.GetExistingScoreAsync(request.DefenseId, councilMember.Id);

            if (existingScore != null)
            {
                existingScore.Score = calculatedScore;
                existingScore.PresentationScore = request.PresentationScore;
                existingScore.DemoScore = request.DemoScore;
                existingScore.QAScore = request.QAScore;
                existingScore.Comment = request.Comment;
                await _defenseRepository.UpdateScoreAsync(existingScore);
            }
            else
            {
                var newScore = new DefenseScore
                {
                    DefenseId = request.DefenseId,
                    CouncilMemberId = councilMember.Id,
                    Score = calculatedScore,
                    PresentationScore = request.PresentationScore,
                    DemoScore = request.DemoScore,
                    QAScore = request.QAScore,
                    Comment = request.Comment,
                    IsPublished = false
                };
                await _defenseRepository.AddScoreAsync(newScore);
            }

            // --- THÊM LOGIC CẬP NHẬT TRẠNG THÁI TẠI ĐÂY ---
            // Giả sử bạn có truy cập vào _context hoặc qua Repository
            var schedule = await _context.DefenseSchedules.FindAsync(request.DefenseId);
            if (schedule != null && schedule.Status != "Completed")
            {
                schedule.Status = "Completed";
                await _defenseRepository.UpdateDefenseScheduleAsync(schedule);
            }

            return await _defenseRepository.SaveChangesAsync() > 0;
        }

        public async Task<DefenseScoreDto?> GetMemberEvaluationAsync(int defenseId, int userId)
        {
            var councilMember = await _defenseRepository.GetCouncilMemberByUserAndDefenseAsync(userId, defenseId);
            if (councilMember == null) return null;

            var score = await _defenseRepository.GetExistingScoreAsync(defenseId, councilMember.Id);
            if (score == null) return null;

            return new DefenseScoreDto
            {
                Score = score.Score ?? 0,
                PresentationScore = score.PresentationScore,
                DemoScore = score.DemoScore,
                QAScore = score.QAScore,
                Comment = score.Comment
            };
        }

        public async Task<IEnumerable<object>> GetAssignedDefensesAsync(int userId)
        {
            // Truy vấn dựa trên cấu trúc bảng DefenseSchedule và CouncilMember trong DB của bạn
            return await _context.DefenseSchedules
                .Include(ds => ds.Group)
                .Include(ds => ds.Group.Topic) // Topic liên kết qua Group
                .Where(ds => _context.CouncilMembers
                    .Any(cm => cm.CouncilId == ds.CouncilId && cm.UserId == userId))
                .Select(ds => new {
                    defenseId = ds.DefenseId,
                    groupName = ds.Group.GroupName,
                    topicTitle = ds.Group.Topic != null ? ds.Group.Topic.Title : "No Topic",
                    scheduledDate = ds.StartTime,
                    endTime = ds.EndTime,
                    room = ds.Room,
                    status = ds.Status
                })
                .ToListAsync();
        }
    }
}