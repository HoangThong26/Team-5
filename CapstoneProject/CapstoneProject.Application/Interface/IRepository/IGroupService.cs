using CapstoneProject.Application.DTO;
using System.Threading.Tasks;

namespace CapstoneProject.Application.Interface.IService
{
    public interface IGroupService
    {
        Task<string> CreateGroupAsync(int userId, CreateGroupRequest request);
        Task<GroupDetailResponse?> GetGroupDetailsAsync(int groupId);
        Task<string> InviteMemberAsync(int leaderId, InviteMemberRequest request);
        Task<string> AcceptInviteAsync(int invitationId);
        
    }
}