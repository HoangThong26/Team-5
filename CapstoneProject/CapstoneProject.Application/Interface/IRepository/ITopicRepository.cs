using CapstoneProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.Interface.IRepository
{
    public interface ITopicRepository
    {
        Task<bool> GroupExistsAsync(int groupId);
        Task<bool> IsUserInGroupAsync(int groupId, int userId);

        Task<Topic?> GetByGroupIdAsync(int groupId);
        Task AddTopicAsync(Topic topic);
        Task AddVersionAsync(TopicVersion version);
        Task SaveChangesAsync();
        Task<Topic?> GetByIdAsync(int topicId);
        Task<TopicVersion?> GetLatestVersionAsync(int topicId);

    }
}
