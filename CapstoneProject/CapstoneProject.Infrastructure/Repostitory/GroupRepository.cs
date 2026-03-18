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
            return await _context.GroupMembers.AnyAsync(m => m.UserId == userId);
        }

        public async Task<Group> CreateGroupWithLeaderAsync(Group group, GroupMember member)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Groups.AddAsync(group);
                await _context.SaveChangesAsync();
                member.GroupId = group.GroupId;
                await _context.GroupMembers.AddAsync(member);
                await _context.SaveChangesAsync();
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
                var invite = await _context.GroupInvitations.FindAsync(invitationId);
                if (invite == null || invite.Status != "Pending")
                    return "The invitation is invalid or has already been processed.";

                bool alreadyInGroup = await _context.GroupMembers.AnyAsync(m => m.UserId == invite.ReceiverId);
                if (alreadyInGroup) return "You are already a member of another group.";

                int checkCountBefore = await _context.GroupMembers.CountAsync(m => m.GroupId == invite.GroupId);
                if (checkCountBefore >= 5)
                {
                    invite.Status = "Expired";
                    _context.GroupInvitations.Update(invite);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return "Too late! The group has already reached the maximum capacity of 5 members.";
                }

                var newMember = new GroupMember
                {
                    GroupId = (int)invite.GroupId,
                    UserId = (int)invite.ReceiverId,
                    JoinedAt = DateTime.Now,
                    RoleInGroup = "Member"
                };
                await _context.GroupMembers.AddAsync(newMember);

                invite.Status = "Accepted";
                _context.GroupInvitations.Update(invite);
                await _context.SaveChangesAsync();

   
                int memberCount = await _context.GroupMembers.CountAsync(m => m.GroupId == invite.GroupId);

                string statusNotification = "";
                if (memberCount >= 4)
                {
                    bool hasMentor = await _context.MentorAssignments.AnyAsync(a => a.GroupId == invite.GroupId);

                    if (!hasMentor)
                    {
                        var assignedMentor = await _context.Users
                            .Where(u => u.Role == "Mentor")
                            .Select(u => new
                            {
                                Mentor = u,
                                GroupCount = _context.MentorAssignments.Count(ma => ma.MentorId == u.UserId)
                            })
                            .OrderBy(x => x.GroupCount)
                            .ThenBy(x => Guid.NewGuid())
                            .Select(x => x.Mentor)
                            .FirstOrDefaultAsync();

                        if (assignedMentor != null)
                        {
                            var assignment = new MentorAssignment
                            {
                                GroupId = invite.GroupId,
                                MentorId = assignedMentor.UserId,
                                AssignedAt = DateTime.Now
                            };
                            await _context.MentorAssignments.AddAsync(assignment);
                            statusNotification = " A mentor has been assigned automatically!";

                            hasMentor = true;
                        }
                    }

                    if (hasMentor)
                    {
                        var group = await _context.Groups.FindAsync(invite.GroupId);
                        if (group != null && group.Status == "Forming")
                        {
                            group.Status = "Active";
                            _context.Groups.Update(group);
                            statusNotification += " The group is now Active!";
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return $"Accepted the invitation successfully!{statusNotification}";
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
                // Thêm dòng Include MentorAssignment này vào:
                .Include(g => g.MentorAssignment)
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

        public async Task<bool> UpdateInvitationStatusAsync(GroupInvitation invitation)
        {
            try
            {
                _context.GroupInvitations.Update(invitation);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<GroupMember?> GetGroupMemberAsync(int groupId, int userId)
        {
            return await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);
        }

        public async Task<bool> RemoveGroupMemberAsync(GroupMember member)
        {
            try
            {
                _context.GroupMembers.Remove(member);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<Group>> GetAllGroupsWithDetailsAsync()
        {
            return await _context.Groups
                .Include(g => g.GroupMembers)
                    .ThenInclude(m => m.User)
                .Include(g => g.MentorAssignment)
                    .ThenInclude(ma => ma.Mentor)
                .ToListAsync();
        }

        public async Task<bool> DeleteGroupAsync(int groupId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var group = await _context.Groups.FindAsync(groupId);
                if (group == null) return false;

                // 1. Xóa Topic & Versions
                var topic = await _context.Topics.FirstOrDefaultAsync(t => t.GroupId == groupId);
                if (topic != null)
                {
                    var versions = _context.TopicVersions.Where(v => v.TopicId == topic.TopicId);
                    _context.TopicVersions.RemoveRange(versions);
                    _context.Topics.Remove(topic);
                }

                // 2. Xóa Reports & Evaluations
                var reports = await _context.WeeklyReports.Where(r => r.GroupId == groupId).ToListAsync();
                foreach (var report in reports)
                {
                    var evaluations = _context.WeeklyEvaluations.Where(e => e.ReportId == report.ReportId);
                    _context.WeeklyEvaluations.RemoveRange(evaluations);
                }
                _context.WeeklyReports.RemoveRange(reports);

                var schedules = await _context.DefenseSchedules.Where(s => s.GroupId == groupId).ToListAsync();
                foreach (var s in schedules)
                {
                    var scores = _context.DefenseScores.Where(ds => ds.DefenseId == s.DefenseId);
                    _context.DefenseScores.RemoveRange(scores);
                }
                _context.DefenseSchedules.RemoveRange(schedules);

                _context.MentorRequests.RemoveRange(_context.MentorRequests.Where(mr => mr.GroupId == groupId));
                _context.GradeComplaints.RemoveRange(_context.GradeComplaints.Where(gc => gc.GroupId == groupId));
                _context.GroupMembers.RemoveRange(_context.GroupMembers.Where(m => m.GroupId == groupId));
                _context.GroupInvitations.RemoveRange(_context.GroupInvitations.Where(i => i.GroupId == groupId));
                _context.MentorAssignments.RemoveRange(_context.MentorAssignments.Where(a => a.GroupId == groupId));
                _context.FinalGrades.RemoveRange(_context.FinalGrades.Where(f => f.GroupId == groupId));
                _context.Groups.Remove(group);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<int?> GetGroupLeaderIdAsync(int groupId)
        {
            return await _context.Groups
                .Where(g => g.GroupId == groupId)
                .Select(g => g.LeaderId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> RemoveMentorFromGroupAsync(int groupId)
        {
            var assignment = await _context.MentorAssignments.FirstOrDefaultAsync(m => m.GroupId == groupId);
            if (assignment == null) return false;

            _context.MentorAssignments.Remove(assignment);
            var group = await _context.Groups.FindAsync(groupId);
            if (group != null && group.Status == "Active")
            {
                group.Status = "Forming";
                _context.Groups.Update(group);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateGroupAsync(Group group)
        {
            _context.Groups.Update(group);
            await _context.SaveChangesAsync();
        }
    }
}