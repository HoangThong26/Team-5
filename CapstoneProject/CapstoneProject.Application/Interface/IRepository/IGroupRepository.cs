using CapstoneProject.Domain.Entities;
using System.Threading.Tasks;

namespace CapstoneProject.Application.Interface.IRepository
{
    public interface IGroupRepository
    {
        Task<bool> IsUserInAnyGroupAsync(int userId);
        Task<Group?> GetGroupByIdAsync(int groupId);
        // Thêm 3 hàm này vào dưới cùng của interface
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> HasPendingInvitationAsync(int groupId, int receiverId);
        Task<GroupInvitation> AddInvitationAsync(GroupInvitation invitation);
        Task<Group> CreateGroupWithLeaderAsync(Group group, GroupMember member);
        Task<int> GetMemberCountAsync(int groupId); // Chuẩn bị sẵn cho chức năng Thêm thành viên
    }
}