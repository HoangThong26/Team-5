using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Application.Interface.IService;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Infrastructure.Services
{
    public class TopicService : ITopicService
    {
        private readonly ITopicRepository _topicRepository;

        public TopicService(ITopicRepository topicRepository)
        {
            _topicRepository = topicRepository;
        }

        public async Task SubmitTopicAsync(int userId, TopicSubmitRequest request)
        {
            var isMember = await _topicRepository.IsUserInGroupAsync(request.GroupId, userId);
            if (!isMember) throw new Exception("You are not a member of this group.");

            var topic = await _topicRepository.GetByGroupIdAsync(request.GroupId);

            if (topic == null)
            {
                topic = new Topic
                {
                    GroupId = request.GroupId,
                    Title = request.Title,
                    Description = request.Description,
                    Status = "Pending",
                    CurrentVersion = 1,
                    CreatedAt = DateTime.UtcNow
                };
                await _topicRepository.AddTopicAsync(topic);
            }
            else
            {
                if (topic.Status == "Approved" || topic.Status == "Active")
                    throw new Exception("Topic already approved. Cannot resubmit.");

                topic.CurrentVersion += 1;
                topic.Title = request.Title;
                topic.Description = request.Description;
                topic.Status = "Pending";
            }

            await _topicRepository.SaveChangesAsync();

            var version = new TopicVersion
            {
                TopicId = topic.TopicId,
                VersionNumber = topic.CurrentVersion,
                Title = request.Title,
                Description = request.Description,
                Status = "Submitted",
                SubmittedAt = DateTime.UtcNow
            };
            await _topicRepository.AddVersionAsync(version);
            await _topicRepository.SaveChangesAsync();
        }

        public async Task EditTopicAsync(int userId, int topicId, TopicUpdateDto request)
        {
            var topic = await _topicRepository.GetByIdAsync(topicId);
            if (topic == null) throw new Exception("Topic not found.");

            var isMember = await _topicRepository.IsUserInGroupAsync(topic.GroupId ?? 0, userId);
            if (!isMember) throw new Exception("You are not authorized to edit this topic.");

            // Rule US-12: Only allow editing if status is Pending
            if (topic.Status != "Pending")
                throw new Exception("Only topics with 'Pending' status can be edited.");

            topic.Title = request.Title;
            topic.Description = request.Description;

            // Update the latest version as well
            var latestVersion = await _topicRepository.GetLatestVersionAsync(topicId);
            if (latestVersion != null && latestVersion.Status == "Submitted")
            {
                latestVersion.Title = request.Title;
                latestVersion.Description = request.Description;
            }

            await _topicRepository.SaveChangesAsync();
        }

        public async Task ApproveTopicAsync(int reviewerId, TopicApprovalRequest request)
        {
            // Existing approval logic...
        }
    }
}