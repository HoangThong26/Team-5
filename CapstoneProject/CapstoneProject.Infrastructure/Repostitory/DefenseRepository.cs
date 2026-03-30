using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Infrastructure.Database.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Infrastructure.Repository
{
    public class DefenseRepository : IDefenseRepository
    {
        private readonly ApplicationDbContext _context;

        public DefenseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DefenseSchedule?> GetDefenseByIdAsync(int defenseId)
        {
            return await _context.DefenseSchedules
                .Include(d => d.Group)
                .FirstOrDefaultAsync(d => d.DefenseId == defenseId);
        }

        // Kiểm tra xem User (Giảng viên) có thuộc Hội đồng của lịch bảo vệ này không
        public async Task<CouncilMember?> GetCouncilMemberByUserAndDefenseAsync(int userId, int defenseId)
        {
            var defense = await _context.DefenseSchedules.FindAsync(defenseId);
            if (defense == null) return null;

            return await _context.CouncilMembers
                .FirstOrDefaultAsync(cm => cm.UserId == userId && cm.CouncilId == defense.CouncilId);
        }

        public async Task<DefenseScore?> GetExistingScoreAsync(int defenseId, int councilMemberId)
        {
            return await _context.DefenseScores
                .FirstOrDefaultAsync(s => s.DefenseId == defenseId && s.CouncilMemberId == councilMemberId);
        }

        public async Task AddScoreAsync(DefenseScore score)
        {
            await _context.DefenseScores.AddAsync(score);
        }

        public async Task UpdateScoreAsync(DefenseScore score)
        {
            _context.DefenseScores.Update(score);
            await Task.CompletedTask;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}