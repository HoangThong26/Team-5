using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Infrastructure.Database.AppDbContext;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CapstoneProject.Infrastructure.Repostitory
{
    public class GroupRepository : IGroupRepository
    {
        private readonly ApplicationDbContext _context;

        public GroupRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsUserInAnyGroupAsync(int userId)
        {
            // Kiểm tra User đã có trong nhóm nào chưa
            return await _context.GroupMembers.AnyAsync(m => m.UserId == userId);
        }

        public async Task<Group> CreateGroupWithLeaderAsync(Group group, GroupMember member)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Lưu Group
                await _context.Groups.AddAsync(group);
                await _context.SaveChangesAsync();

                // 2. Gán GroupId cho Leader và lưu Member
                member.GroupId = group.GroupId;
                await _context.GroupMembers.AddAsync(member);
                await _context.SaveChangesAsync();

                // 3. Chốt giao dịch
                await transaction.CommitAsync();
                return group;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> GetMemberCountAsync(int groupId)
        {
            // Đếm số lượng thành viên hiện tại của một nhóm
            return await _context.GroupMembers.CountAsync(m => m.GroupId == groupId);
        }
        public async Task<Group?> GetGroupByIdAsync(int groupId)
        {
            // Dùng Include để lấy Group kèm theo danh sách GroupMembers
            return await _context.Groups
                .Include(g => g.GroupMembers)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);
        }
        // Copy 3 hàm này dán vào bên trong class GroupRepository
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> HasPendingInvitationAsync(int groupId, int receiverId)
        {
            // Kiểm tra xem lời mời đã tồn tại và đang ở trạng thái Pending chưa
            return await _context.GroupInvitations
                .AnyAsync(i => i.GroupId == groupId && i.ReceiverId == receiverId && i.Status == "Pending");
        }

        public async Task<GroupInvitation> AddInvitationAsync(GroupInvitation invitation)
        {
            await _context.GroupInvitations.AddAsync(invitation);
            await _context.SaveChangesAsync();
            return invitation;
        }
    }
}