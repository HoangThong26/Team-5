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

        Task<GroupMember?> GetGroupMemberAsync(int groupId, int userId);
        Task<bool> RemoveGroupMemberAsync(GroupMember member);
        Task<List<Group>> GetAllGroupsWithDetailsAsync();
        Task<bool> DeleteGroupAsync(int groupId);
        Task<bool> RemoveMentorFromGroupAsync(int groupId);
        Task<int?> GetGroupLeaderIdAsync(int groupId);
        Task UpdateGroupAsync(Group group);
    }
}