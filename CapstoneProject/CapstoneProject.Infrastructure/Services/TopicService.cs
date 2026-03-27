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
            var isLeader = await _topicRepository.IsGroupLeaderAsync(request.GroupId, userId);
            if (!isLeader)
            {
                throw new Exception("Access Denied: Only the Group Leader has permission to submit or update the topic.");
            }

            var hasMentor = await _topicRepository.HasMentorAssignedAsync(request.GroupId);
            if (!hasMentor)
            {
                throw new Exception("Assignment Error: Your group has not been assigned a mentor yet. Please contact the Admin.");
            }

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
                string[] finalizedStatuses = { "Approved", "Active", "Completed" };
                if (finalizedStatuses.Contains(topic.Status))
                {
                    throw new Exception("Submission Blocked: This topic has already been approved or is currently active. Changes are no longer allowed.");
                }

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

            if (topic.Status != "Pending")
                throw new Exception("Only topics with 'Pending' status can be edited.");

            topic.Title = request.Title;
            topic.Description = request.Description;
            var latestVersion = await _topicRepository.GetLatestVersionAsync(topicId);
            if (latestVersion != null && latestVersion.Status == "Submitted")
            {
                latestVersion.Title = request.Title;
                latestVersion.Description = request.Description;
            }

            await _topicRepository.SaveChangesAsync();
        }

        public async Task<TopicDto?> GetTopicByGroupIdAsync(int groupId)
        {
            var topic = await _topicRepository.GetByGroupIdAsync(groupId);
            if (topic == null) return null;
            var latestVersion = await _topicRepository.GetLatestVersionAsync(topic.TopicId);

            return new TopicDto
            {
                TopicId = topic.TopicId,
                GroupId = topic.GroupId ?? 0,
                Title = topic.Title ?? string.Empty,
                Description = topic.Description ?? string.Empty,
                Status = topic.Status ?? string.Empty,
                ReviewComment = latestVersion?.ReviewComment,
                VersionId = latestVersion?.Id ?? 0
            };
        }

        public async Task ApproveTopicAsync(int reviewerId, TopicApprovalRequest request)
        {
            var version = await _topicRepository.GetVersionByIdAsync(request.VersionId);
            if (version == null) throw new Exception("Topic version not found.");
            var topic = await _topicRepository.GetByIdAsync(version.TopicId.Value);
            if (topic == null) throw new Exception("Original topic not found.");

            var isMentor = await _topicRepository.IsMentorOfGroupAsync(topic.GroupId ?? 0, reviewerId);
            if (!isMentor) throw new Exception("Access denied. You are not authorized to review this group's topic.");

            if (request.Status == "Approved")
            {
                topic.Status = "Approved";    
                version.Status = "Approved"; 
            }
            else if (request.Status == "Rejected")
            {
                topic.Status = "Rejected";   
                version.Status = "Rejected"; 
            }
            else
            {
                throw new Exception("The approval status is invalid.");
            }
            version.ReviewedBy = reviewerId;
            version.ReviewComment = request.ReviewComment;

            await _topicRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<TopicDto>> GetAllTopicsForMentorAsync(int mentorId)
        {
            var versions = await _topicRepository.GetTopicVersionsByMentorAsync(mentorId);

            return versions.Select(v => new TopicDto
            {
                TopicId = v.TopicId ?? 0,
                VersionId = v.Id,
                Title = v.Title,
                Description = v.Description,
                Status = v.Topic?.Status ?? "Pending",
                SubmittedAt = v.SubmittedAt,
                GroupId = v.Topic?.GroupId ?? 0,
                GroupName = v.Topic?.Group?.GroupName ?? "N/A"
            });
        }

        public async Task<int?> GetMentorIdByGroupIdAsync(int groupId)
        {
            return await _topicRepository.GetMentorIdByGroupIdAsync(groupId);
        }

        public async Task<int?> GetGroupIdByTopicIdAsync(int topicId)
        {
            return await _topicRepository.GetGroupIdByTopicIdAsync(topicId);
        }
        public async Task<string?> GetMentorEmailByGroupId(int groupId)
        {
            var email = await _topicRepository.GetMentorEmailByGroupIdAsync(groupId);

            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            return email;
        }

        public async Task<ServiceResponse<object>> GetMentorProposalBoardAsync(int mentorId)
        {
            var response = new ServiceResponse<object>();
            try
            {
                var versions = await _topicRepository.GetMentorBoardVersionsAsync(mentorId);

                var allList = versions.Select(v => new TopicBoardDto
                {
                    VersionId = v.Id,
                    TeamName = v.Topic?.Group?.GroupName ?? "No Team",
                    TopicName = v.Topic?.Title ?? "No Topic Name",
                    Description = v.Description,
                    Status = v.Status,
                    SubmittedAt = v.SubmittedAt
                }).ToList();
                response.Data = new
                {
                    All = allList,
                    Pending = allList.Where(x => x.Status == "Submitted" || x.Status == "Pending").ToList(),
                    Approved = allList.Where(x => x.Status == "Approved").ToList(),
                    Rejected = allList.Where(x => x.Status == "Rejected").ToList()
                };

                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}