using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Application.Interface.IService;

public class CouncilService : ICouncilService
{
    private readonly ICouncilRepository _councilRepo;

    public CouncilService(ICouncilRepository councilRepo)
    {
        _councilRepo = councilRepo;
    }

    public async Task<ServiceResponse<int>> CreateCouncilFullAsync(CouncilCreateRequest request)
    {
        var response = new ServiceResponse<int>();
        try
        {
            int memberCount = request.MemberIds?.Count ?? 0;
            if (memberCount < 3 || memberCount > 6)
            {
                response.Success = false;
                response.Message = $"Council members count must be between 3 and 6. (Current: {memberCount})";
                return response;
            }


            var existingStaffNames = await _councilRepo.GetStaffAlreadyInCouncilAsync(request.MemberIds);
            if (existingStaffNames.Any())
            {
                response.Success = false;
                response.Message = $"The following staffs are already assigned to other councils: {string.Join(", ", existingStaffNames)}";
                return response;
            }

            int councilId = await _councilRepo.CreateCouncilWithMembersAsync(request.Name, request.MemberIds);

            response.Data = councilId;
            response.Message = "Council and members created successfully.";
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = $"Error creating council: {ex.Message}";
        }
        return response;
    }

    public async Task<ServiceResponse<List<StaffDto>>> GetStaffsForCouncilAsync()
    {
        try
        {
            var staffs = await _councilRepo.GetAvailableStaffsAsync();
            var data = staffs.Select(s => new StaffDto
            {
                UserId = s.UserId,
                FullName = s.FullName,
                Email = s.Email
            }).ToList();

            return new ServiceResponse<List<StaffDto>> { Data = data };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<List<StaffDto>> { Success = false, Message = ex.Message };
        }
    }

    public async Task<BaseResponse<List<StaffSearchDto>>> SearchStaffForCouncilAsync(string searchTerm)
    {
        var users = await _councilRepo.SearchAvailableStaffAsync(searchTerm);

        var data = users.Select(u => new StaffSearchDto
        {
            UserId = u.UserId,
            FullName = u.FullName,
            Email = u.Email,
            Role = u.Role
        }).ToList();

        return new BaseResponse<List<StaffSearchDto>>
        {
            Data = data,
            Success = true,
            Message = $"Tìm thấy {data.Count} nhân sự phù hợp."
        };
    }
}