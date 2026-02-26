using CapstoneProject.Domain.Entities;
using System.Threading.Tasks;

namespace CapstoneProject.Application.Interface.IRepository
{
    public interface IGroupRepository
    {
        Task<bool> IsUserInAnyGroupAsync(int userId);
        Task<Group> CreateGroupWithLeaderAsync(Group group, GroupMember member);
        Task<int> GetMemberCountAsync(int groupId); // Chuẩn bị sẵn cho chức năng Thêm thành viên
    }
}