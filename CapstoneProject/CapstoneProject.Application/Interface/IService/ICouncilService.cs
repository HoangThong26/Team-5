using CapstoneProject.Application.DTO;

namespace CapstoneProject.Application.Interface.IService
{
    public interface ICouncilService
    {
        Task<ServiceResponse<int>> CreateCouncilFullAsync(CouncilCreateRequest request);
        Task<ServiceResponse<List<StaffDto>>> GetStaffsForCouncilAsync();
        Task<BaseResponse<List<StaffSearchDto>>> SearchStaffForCouncilAsync(string searchTerm);
    }
}
