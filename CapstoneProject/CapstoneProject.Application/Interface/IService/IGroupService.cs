using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IRepository;
using System.Threading.Tasks;

namespace CapstoneProject.Application.Interface.IService
{
    public interface IGroupService
    {
        Task<string> CreateGroupAsync(int userId, CreateGroupRequest request);
        Task<GroupDetailResponse?> GetGroupDetailsAsync(int groupId);
        Task<string> InviteMemberAsync(int leaderId, InviteMemberRequest request);
        Task<string> AcceptInviteAsync(int invitationId);
        Task<GroupDetailResponse?> GetMyGroupAsync(int userId);
        Task<string> RejectInviteAsync(int invitationId);
        Task<string> KickMemberAsync(int requesterId, int groupId, int targetUserId);
        Task<List<GroupDetailResponse>> GetAllGroupsForAdminAsync();
        Task<string> DeleteGroupByAdminAsync(int groupId, int currentUserId, string currentUserRole);
        Task<bool> AssignMentorAsync(int groupId, int mentorId);
        Task<string> KickMentorByAdminAsync(int groupId);
    }
}