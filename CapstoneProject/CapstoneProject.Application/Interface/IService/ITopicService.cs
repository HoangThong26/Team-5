using CapstoneProject.Application.DTO;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Interface.IService
{
    public interface ITopicService
    {
        Task SubmitTopicAsync(int userId, TopicSubmitRequest request);
        Task EditTopicAsync(int userId, int topicId, TopicUpdateDto request);
        Task ApproveTopicAsync(int reviewerId, TopicApprovalRequest request);
        Task<IEnumerable<TopicDto>> GetAllTopicsForMentorAsync(int mentorId);
        Task<TopicDto?> GetTopicByGroupIdAsync(int groupId);
        Task<int?> GetMentorIdByGroupIdAsync(int groupId);
        Task<int?> GetGroupIdByTopicIdAsync(int topicId);
        Task<string?> GetMentorEmailByGroupId(int groupId);
        Task<ServiceResponse<object>> GetMentorProposalBoardAsync(int mentorId);
        Task<List<TopicDto>> SearchTopicsAsync(string? keyword, string? status, int? supervisorId);
    }
}
