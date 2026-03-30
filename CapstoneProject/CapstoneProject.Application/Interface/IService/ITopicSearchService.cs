using CapstoneProject.Application.DTO;

namespace CapstoneProject.Application.Interface
{
    public interface ITopicSearchService
    {
        Task<object> SearchGlobalAsync(TopicSearchRequest request);

        // HÀM MỚI CHO MENTOR
        Task<object> SearchForMentorAsync(TopicSearchRequest request, int currentMentorId);
    }
}