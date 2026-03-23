using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Application.Interface.IService;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Service
{
    public class DefenseService : IDefenseService
    {
        private readonly IDefenseRepository _defenseRepository;

        public DefenseService(IDefenseRepository defenseRepository)
        {
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
                Comment = score.Comment
            };
        }
    }
}