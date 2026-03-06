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
                // 1. Lưu Group để lấy GroupId
                await _context.Groups.AddAsync(group);
                await _context.SaveChangesAsync();

                // 2. Gán GroupId cho Leader và lưu vào bảng GroupMembers
                member.GroupId = group.GroupId;
                await _context.GroupMembers.AddAsync(member);
                await _context.SaveChangesAsync();

                // CHÚ Ý: Không gán Mentor ở đây vì lúc này nhóm mới có 1 người.

                await transaction.CommitAsync();
                return group;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<string> AcceptInvitationWithMentorCheckAsync(int invitationId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Lấy thông tin lời mời
                var invite = await _context.GroupInvitations.FindAsync(invitationId);
                if (invite == null || invite.Status != "Pending")
                    return "The invitation is invalid or has already been processed.";

                // 2. Kiểm tra xem User này đã có nhóm khác chưa (Double check an toàn)
                bool alreadyInGroup = await _context.GroupMembers.AnyAsync(m => m.UserId == invite.ReceiverId);
                if (alreadyInGroup) return "You are already a member of another group.";

                // 3. Thêm thành viên mới vào GroupMembers
                var newMember = new GroupMember
                {
                    GroupId =(int) invite.GroupId,
                    UserId = (int) invite.ReceiverId,
                    JoinedAt = DateTime.Now,
                    RoleInGroup = "Member"
                };
                await _context.GroupMembers.AddAsync(newMember);

                // 4. Cập nhật trạng thái lời mời
                invite.Status = "Accepted";
                _context.GroupInvitations.Update(invite);
                await _context.SaveChangesAsync();

                // 5. ĐẾM SỐ THÀNH VIÊN HIỆN TẠI (Tính cả người vừa vào)
                int memberCount = await _context.GroupMembers.CountAsync(m => m.GroupId == invite.GroupId);

                string mentorNotification = "";

                // 6. NẾU ĐỦ 4 NGƯỜI THÌ GÁN MENTOR
                if (memberCount == 4)
                {
                    // Kiểm tra xem nhóm đã có Mentor chưa (đề phòng)
                    bool hasMentor = await _context.MentorAssignments.AnyAsync(a => a.GroupId == invite.GroupId);

                    if (!hasMentor)
                    {
                        // Tìm Mentor ngẫu nhiên đang hoạt động
                        var randomMentor = await _context.Users
                            .Where(u => u.Role == "Mentor" && u.Status == "Active")
                            .OrderBy(u => Guid.NewGuid())
                            .FirstOrDefaultAsync();

                        if (randomMentor != null)
                        {
                            var assignment = new MentorAssignment
                            {
                                GroupId = invite.GroupId,
                                MentorId = randomMentor.UserId,
                                AssignedAt = DateTime.Now
                            };
                            await _context.MentorAssignments.AddAsync(assignment);
                            mentorNotification = "The group has reached 4 members, and a mentor has been assigned automatically!";
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return $"Accepted the invitation successfully!{mentorNotification}";
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // Để Service xử lý hoặc Log lỗi
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

        // 1. Lấy thông tin thư mời
        public async Task<GroupInvitation?> GetInvitationByIdAsync(int invitationId)
        {
            return await _context.GroupInvitations.FirstOrDefaultAsync(i => i.InvitationId == invitationId);
        }

        // 2. Cập nhật trạng thái thư mời (từ Pending sang Accepted)
        public async Task UpdateInvitationAsync(GroupInvitation invitation)
        {
            _context.GroupInvitations.Update(invitation);
            await _context.SaveChangesAsync();
        }

        // 3. Thêm sinh viên vào nhóm
        public async Task AddGroupMemberAsync(GroupMember member)
        {
            await _context.GroupMembers.AddAsync(member);
            await _context.SaveChangesAsync();
        }
    }
}