using CapstoneProject.Application.DTO;

namespace CapstoneProject.Application.Interface.IService
{
    public interface IDefenseService
    {
        Task<DefenseRegistrationStatusDto?> GetMyRegistrationStatusAsync(int userId);
        Task RegisterMyGroupAsync(int userId);
        Task<List<DefenseRegistrationItemDto>> GetDefenseRegistrationsAsync();
        Task<List<DefenseCommitteeDto>> GetDefenseCommitteesAsync();
        Task<List<CouncilUserDto>> GetCouncilUsersAsync();
        Task CreateDefenseScheduleAsync(CreateDefenseScheduleRequest request);
        Task UpdateDefenseScheduleAsync(int defenseId, UpdateDefenseScheduleRequest request);
        Task<bool> SubmitEvaluationAsync(DefenseEvaluationRequest request);
        Task<DefenseScoreDto?> GetMemberEvaluationAsync(int defenseId, int userId);
        Task<IEnumerable<object>> GetAssignedDefensesAsync(int userId);
    }
}