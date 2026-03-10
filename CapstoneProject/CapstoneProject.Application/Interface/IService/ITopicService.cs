using CapstoneProject.Application.DTO;

namespace CapstoneProject.Application.Interface.IService
{
    public interface ITopicService
    {
        Task SubmitTopicAsync(int userId, TopicSubmitRequest request);
        Task EditTopicAsync(int userId, int topicId, TopicUpdateDto request);
        Task ApproveTopicAsync(int reviewerId, TopicApprovalRequest request);
    }
}