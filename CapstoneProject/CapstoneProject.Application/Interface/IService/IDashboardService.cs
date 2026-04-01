using CapstoneProject.Application.DTO;
using System.Threading.Tasks;

namespace CapstoneProject.Application.Interface.IService
{
    public interface IDashboardService
    {
        Task<DashboardStatisticsResponse> GetAdminDashboardStatsAsync();
    }
}