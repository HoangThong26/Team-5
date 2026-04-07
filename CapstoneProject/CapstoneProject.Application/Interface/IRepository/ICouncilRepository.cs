using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Interface.IRepository
{
    public interface ICouncilRepository
    {
        Task<int> CreateCouncilWithMembersAsync(string councilName, List<int> memberIds);
        Task<List<User>> GetAvailableStaffsAsync();

        // Giữ lại các hàm đơn lẻ nếu bạn vẫn cần dùng cho mục đích khác
        Task<Council> CreateCouncilAsync(Council council);
        Task AddMembersAsync(List<CouncilMember> members);
        Task<List<User>> SearchAvailableStaffAsync(string searchTerm);
        Task<List<string>> GetStaffAlreadyInCouncilAsync(List<int> userIds);
    }
}
