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
        private readonly ApplicationDbContext _context;

        public DefenseService(ApplicationDbContext context, IDefenseRepository defenseRepository)
        {
            _context = context;
            _defenseRepository = defenseRepository;
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