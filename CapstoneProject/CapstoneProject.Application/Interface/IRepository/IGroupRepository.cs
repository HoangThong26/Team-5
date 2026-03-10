using CapstoneProject.Domain.Entities;
using System.Threading.Tasks;

namespace CapstoneProject.Application.Interface.IRepository
{
    public interface IGroupRepository
    {
        Task<bool> IsUserInAnyGroupAsync(int userId);
        Task<Group?> GetGroupByIdAsync(int groupId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> HasPendingInvitationAsync(int groupId, int receiverId);
        Task<GroupInvitation> AddInvitationAsync(GroupInvitation invitation);
        Task<Group> CreateGroupWithLeaderAsync(Group group, GroupMember member);
        Task<int> GetMemberCountAsync(int groupId); 

        Task<GroupInvitation?> GetInvitationByIdAsync(int invitationId);
        Task UpdateInvitationAsync(GroupInvitation invitation);
        Task AddGroupMemberAsync(GroupMember member);
        Task<string> AcceptInvitationWithMentorCheckAsync(int invitationId);
        Task<Group?> GetGroupByUserIdAsync(int userId);
        Task<Group?> GetGroupWithDetailsByUserIdAsync(int userId);
        Task<bool> UpdateInvitationStatusAsync(GroupInvitation invitation);
    }
}