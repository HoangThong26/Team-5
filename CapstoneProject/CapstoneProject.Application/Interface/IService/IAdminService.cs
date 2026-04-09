using CapstoneProject.Application.DTO;

namespace CapstoneProject.Application.Interface.IService
{
    public interface IAdminService
    {
        Task<string> CreateUserByRoleAsync(AdminCreateUserRequest request);
        Task<List<UserResponseDto>> GetAllUsersAsync(int currentUserId);
        Task DeleteAsync(int userId);
        Task UnlockAccountAsync(int userId);
        Task<List<AdminUserResponse>> SearchUsersAsync(string keyword, int currentUserId);
        Task SetupTimelineAsync(DateTime startDate);
        Task<int> ImportUsersFromExcelAsync(Stream excelStream);
        Task<byte[]> ExportStudentsToExcelAsync();
    }
}
