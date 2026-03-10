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
        Task<int> SaveChangesAsync();
    }
}