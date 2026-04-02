using CapstoneProject.Application.DTOs;

namespace CapstoneProject.Application.Interfaces
{
    public interface IStatisticService
    {
        Task<PassFailStatisticDto> GetPassFailStatisticsAsync(int? semesterId, int? majorId);
    }
}