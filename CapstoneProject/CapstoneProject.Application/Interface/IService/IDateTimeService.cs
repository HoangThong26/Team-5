using CapstoneProject.Application.DTO;

namespace CapstoneProject.Application.Interface.IService
{
    public interface IDateTimeService
    {
        Task<ServiceResponse<DeadlineResponse>> GetWeeklyDeadlineAsync();
    }
}
