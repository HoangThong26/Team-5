using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Interface.IRepository
{
    public interface IDefenseRepository
    {
        Task<DefenseSchedule?> GetDefenseByIdAsync(int defenseId);
        Task<CouncilMember?> GetCouncilMemberByUserAndDefenseAsync(int userId, int defenseId);
        Task<DefenseScore?> GetExistingScoreAsync(int defenseId, int councilMemberId);
        Task AddScoreAsync(DefenseScore score);
        Task UpdateScoreAsync(DefenseScore score);
        Task<int> SaveChangesAsync();
    }
}