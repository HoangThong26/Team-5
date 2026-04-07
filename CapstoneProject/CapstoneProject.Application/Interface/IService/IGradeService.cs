using CapstoneProject.Application.DTO;

namespace CapstoneProject.Application.Interface.IService
{
    public interface IGradeService
    {
        Task<decimal> CalculateAndSaveFinalGrade(int groupId);
        Task<List<FinalGradeResponseDto>> GetAllGradesAsync();
        Task PublishGradeAsync(int groupId);
    }
}
//hihi 