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
            // 1. Kiểm tra quyền thành viên
            var isMember = await _topicRepository.IsUserInGroupAsync(request.GroupId, userId);
            if (!isMember) throw new Exception("You are not a member of this group.");

            // 2. Kiểm tra xem nhóm đã có Mentor chưa (Nếu không có, submit sẽ không ai thấy)
            // Bạn nên thêm hàm CheckMentor vào Repository
            var hasMentor = await _topicRepository.HasMentorAssignedAsync(request.GroupId);
            if (!hasMentor) throw new Exception("Nhóm chưa được phân công Mentor. Vui lòng liên hệ Admin.");

            var topic = await _topicRepository.GetByGroupIdAsync(request.GroupId);

            if (topic == null)
            {
                topic = new Topic
                {
                    GroupId = request.GroupId,
                    Title = request.Title,
                    Description = request.Description,
                    Status = "Pending", // Trạng thái bảng Topic là Pending
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

            // LƯU LẦN 1: Để chắc chắn Topic đã có ID từ Database
            await _topicRepository.SaveChangesAsync();

            // 3. Tạo Version mới - ĐÂY LÀ DỮ LIỆU MENTOR SẼ TRUY VẤN
            var version = new TopicVersion
            {
                TopicId = topic.TopicId, // Đảm bảo lấy ID vừa sinh ra hoặc đã tồn tại
                VersionNumber = topic.CurrentVersion,
                Title = request.Title,
                Description = request.Description,
                Status = "Submitted", // Mentor tìm kiếm trạng thái 'Submitted'
                SubmittedAt = DateTime.UtcNow
            };

            await _topicRepository.AddVersionAsync(version);
            await _topicRepository.SaveChangesAsync(); // LƯU LẦN 2: Hoàn tất
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

        public async Task<TopicDto?> GetTopicByGroupIdAsync(int groupId)
        {
            // Giả sử bạn có ITopicRepository đã được inject
            var topic = await _topicRepository.GetByGroupIdAsync(groupId);

            if (topic == null) return null;

            return new TopicDto
            {
                TopicId = topic.TopicId,
                GroupId = topic.GroupId ?? 0,
                Title = topic.Title ?? string.Empty,
                Description = topic.Description ?? string.Empty,
                Status = topic.Status ?? string.Empty
            };
        }

        public async Task ApproveTopicAsync(int reviewerId, TopicApprovalRequest request)
        {
            var version = await _topicRepository.GetVersionByIdAsync(request.VersionId);
            if (version == null)
                throw new Exception("Topic version not found.");
            var topic = await _topicRepository.GetByIdAsync(version.TopicId.Value);
            if (topic == null)
                throw new Exception("Original topic not found.");
            var isMentor = await _topicRepository.IsMentorOfGroupAsync(topic.GroupId ?? 0, reviewerId);
            if (!isMentor)
                throw new Exception("Access denied. You are not authorized to review this group's topic.");
            topic.Status = (request.Status == "Approved") ? "Active" : "Rejected";
            version.Status = request.Status;
            version.ReviewedBy = reviewerId;
            version.ReviewComment = request.ReviewComment;
            version.SubmittedAt = DateTime.UtcNow; 

            await _topicRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<TopicDto>> GetPendingTopicsForMentorAsync(int mentorId)
        {
            var pendingVersions = await _topicRepository.GetPendingTopicVersionsByMentorAsync(mentorId);

            return pendingVersions.Select(v => new TopicDto
            {
                TopicId = v.TopicId ?? 0, 
                VersionId = v.Id,       

                Title = v.Title,
                Description = v.Description,
                Status = v.Status,
                SubmittedAt = v.SubmittedAt,

                GroupId = v.Topic?.GroupId ?? 0,
                GroupName = v.Topic?.Group?.GroupName ?? "N/A"
            });
        }
    }
}