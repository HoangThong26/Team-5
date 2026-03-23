using CapstoneProject.Application.DTO;

namespace CapstoneProject.Application.Interface.IService
{
    public interface IDefenseService
    {
        Task<bool> SubmitEvaluationAsync(DefenseEvaluationRequest request);
        Task<DefenseScoreDto?> GetMemberEvaluationAsync(int defenseId, int userId);
    }
}