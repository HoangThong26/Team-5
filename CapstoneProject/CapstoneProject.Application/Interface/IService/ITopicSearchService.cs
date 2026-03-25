using CapstoneProject.Application.DTO;

namespace CapstoneProject.Application.Interface
{
    public interface ITopicSearchService
    {
        Task<object> SearchGlobalAsync(TopicSearchRequest request);
    }
}