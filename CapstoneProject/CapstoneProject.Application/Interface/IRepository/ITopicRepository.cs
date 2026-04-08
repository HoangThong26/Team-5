using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Interface.IRepository
{
    public interface ITopicRepository
    {
        Task<Topic?> GetByGroupIdAsync(int groupId);
        Task<Topic?> GetByIdAsync(int topicId);
        Task<TopicVersion?> GetLatestVersionAsync(int topicId);
        Task AddTopicAsync(Topic topic);
        Task AddVersionAsync(TopicVersion version);
        Task<bool> GroupExistsAsync(int groupId);
        Task<bool> IsUserInGroupAsync(int groupId, int userId);
        Task<bool> IsMentorOfGroupAsync(int groupId, int mentorId);
        Task<IEnumerable<TopicVersion>> GetPendingTopicVersionsByMentorAsync(int mentorId);
        Task<TopicVersion?> GetVersionByIdAsync(int id);
        Task<bool> HasMentorAssignedAsync(int groupId);
        Task<IEnumerable<TopicVersion>> GetTopicVersionsByMentorAsync(int mentorId);
        Task<bool> IsGroupLeaderAsync(int groupId, int userId);
        Task<int?> GetMentorIdByGroupIdAsync(int groupId);
        Task<int?> GetGroupIdByTopicIdAsync(int topicId);
        Task<int> SaveChangesAsync();
        Task<string?> GetMentorEmailByGroupIdAsync(int groupId);
        Task<IEnumerable<TopicVersion>> GetMentorBoardVersionsAsync(int mentorId);
        Task<bool> IsTopicApprovedForGroupAsync(int groupId);
        Task<List<Topic>> SearchTopicsAsync(string? keyword, string? status, int? supervisorId);
    }
}