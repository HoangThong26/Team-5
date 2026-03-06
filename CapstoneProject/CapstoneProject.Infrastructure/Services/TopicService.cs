using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Application.Interface.IService;
using CapstoneProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

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
            // 1️⃣ Check group tồn tại
            var groupExists = await _topicRepository.GroupExistsAsync(request.GroupId);
            if (!groupExists)
                throw new Exception("Group not found");

            // 2️⃣ Check user thuộc group
            var isMember = await _topicRepository.IsUserInGroupAsync(request.GroupId, userId);
            if (!isMember)
                throw new Exception("You are not a member of this group");

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
                await _topicRepository.SaveChangesAsync();

                var version = new TopicVersion
                {
                    TopicId = topic.TopicId,
                    VersionNumber = 1,
                    Title = request.Title,
                    Description = request.Description,
                    Status = "Submitted"
                };

                await _topicRepository.AddVersionAsync(version);
            }
            else
            {
                if (topic.Status == "Approved")
                    throw new Exception("Topic already approved. Cannot resubmit.");

                topic.CurrentVersion += 1;
                topic.Title = request.Title;
                topic.Description = request.Description;
                topic.Status = "Pending";

                var version = new TopicVersion
                {
                    TopicId = topic.TopicId,
                    VersionNumber = topic.CurrentVersion,
                    Title = request.Title,
                    Description = request.Description,
                    Status = "Submitted"
                };

                await _topicRepository.AddVersionAsync(version);
            }

            await _topicRepository.SaveChangesAsync();
        }


        public async Task ApproveTopicAsync(int reviewerId, TopicApprovalRequest request)
        {
            var topic = await _topicRepository.GetByIdAsync(request.TopicId);

            if (topic == null)
                throw new Exception("Topic not found");

            if (topic.Status != "Pending")
                throw new Exception("Only pending topics can be reviewed");

            var latestVersion = await _topicRepository
                .GetLatestVersionAsync(request.TopicId);

            if (latestVersion == null)
                throw new Exception("Topic version not found");

            if (request.IsApproved)
            {
                topic.Status = "Approved";
                latestVersion.Status = "Approved";


            }
            else
            {
                topic.Status = "Rejected";
                latestVersion.Status = "Rejected";
            }

            latestVersion.ReviewedBy = reviewerId;
            latestVersion.ReviewComment = request.Comment;

            await _topicRepository.SaveChangesAsync();
        }
    }

}
