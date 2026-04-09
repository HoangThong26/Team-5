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

        public async Task<DefenseSchedule?> GetDefenseByGroupIdAsync(int groupId)
        {
            return await _context.DefenseSchedules
                .Include(d => d.Group)
                .FirstOrDefaultAsync(d => d.GroupId == groupId);
        }

        public async Task<List<DefenseSchedule>> GetDefenseRegistrationsAsync()
        {
            return await _context.DefenseSchedules
                .Include(d => d.Group)
                .Include(d => d.Council)
                .Where(d => d.GroupId.HasValue)
                .OrderByDescending(d => d.DefenseId)
                .ToListAsync();
        }

        public async Task<List<Council>> GetAllCouncilsWithMembersAsync()
        {
            return await _context.Councils
                .Include(c => c.CouncilMembers)
                    .ThenInclude(cm => cm.User)
                .OrderBy(c => c.CouncilId)
                .ToListAsync();
        }

        public async Task<Council?> GetCouncilByIdAsync(int councilId)
        {
            return await _context.Councils.FirstOrDefaultAsync(c => c.CouncilId == councilId);
        }

        public async Task<int> GetCouncilMemberCountAsync(int councilId)
        {
            return await _context.CouncilMembers
                .CountAsync(cm => cm.CouncilId == councilId && cm.UserId.HasValue);
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

        public async Task AddDefenseScheduleAsync(DefenseSchedule schedule)
        {
            await _context.DefenseSchedules.AddAsync(schedule);
        }

        public async Task UpdateDefenseScheduleAsync(DefenseSchedule schedule)
        {
            _context.DefenseSchedules.Update(schedule);
            await Task.CompletedTask;
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