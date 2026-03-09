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

                invite.Status = "Accepted";
                _context.GroupInvitations.Update(invite);
                await _context.SaveChangesAsync();
                int memberCount = await _context.GroupMembers.CountAsync(m => m.GroupId == invite.GroupId);

                string mentorNotification = "";

                if (memberCount >= 4)
                {
                    bool hasMentor = await _context.MentorAssignments.AnyAsync(a => a.GroupId == invite.GroupId);

                    if (!hasMentor)
                    {
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
                throw; 
            }
        }

        public async Task<int> GetMemberCountAsync(int groupId)
        {
            return await _context.GroupMembers.CountAsync(m => m.GroupId == groupId);
        }
        public async Task<Group?> GetGroupByIdAsync(int groupId)
        {
            return await _context.Groups
                .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.User) 
                .FirstOrDefaultAsync(g => g.GroupId == groupId);
        }
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> HasPendingInvitationAsync(int groupId, int receiverId)
        {
            return await _context.GroupInvitations
                .AnyAsync(i => i.GroupId == groupId && i.ReceiverId == receiverId && i.Status == "Pending");
        }

        public async Task<GroupInvitation> AddInvitationAsync(GroupInvitation invitation)
        {
            await _context.GroupInvitations.AddAsync(invitation);
            await _context.SaveChangesAsync();
            return invitation;
        }
        public async Task<GroupInvitation?> GetInvitationByIdAsync(int invitationId)
        {
            return await _context.GroupInvitations.FirstOrDefaultAsync(i => i.InvitationId == invitationId);
        }

        public async Task UpdateInvitationAsync(GroupInvitation invitation)
        {
            _context.GroupInvitations.Update(invitation);
            await _context.SaveChangesAsync();
        }

        public async Task AddGroupMemberAsync(GroupMember member)
        {
            await _context.GroupMembers.AddAsync(member);
            await _context.SaveChangesAsync();
        }
        public async Task<Group?> GetGroupByUserIdAsync(int userId)
        {
            return await _context.Groups
                .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.User) 
                .FirstOrDefaultAsync(g => g.GroupMembers.Any(m => m.UserId == userId));
        }

        public async Task<Group?> GetGroupWithDetailsByUserIdAsync(int userId)
        {
            return await _context.Groups
  
                .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.User)

                .Include(g => g.MentorAssignment)
                    .ThenInclude(ma => ma.Mentor) 
                .FirstOrDefaultAsync(g => g.GroupMembers.Any(m => m.UserId == userId));
        }
    }
}