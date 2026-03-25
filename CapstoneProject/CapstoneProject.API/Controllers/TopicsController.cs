using CapstoneProject.API.Hubs;
using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CapstoneProject.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TopicsController : ControllerBase
    {
        private readonly ITopicService _topicService;
        private readonly IGroupService _groupService; 
        private readonly IHubContext<NotificationHub> _hubContext;

        public TopicsController(
            ITopicService topicService,
            IGroupService groupService,
            IHubContext<NotificationHub> hubContext)
        {
            _topicService = topicService;
            _groupService = groupService;
            _hubContext = hubContext;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitTopic([FromBody] TopicSubmitRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
                await _topicService.SubmitTopicAsync(userId, request);
                var mentorEmail = await _topicService.GetMentorEmailByGroupId(request.GroupId);

                if (!string.IsNullOrEmpty(mentorEmail))
                {
                    await _hubContext.Clients.User(mentorEmail).SendAsync("ReceiveNotification", new
                    {
                        type = "TOPIC_SUBMITTED", 
                        message = $"Group {request.GroupId} has submitted a new topic!",
                        groupId = request.GroupId
                    });
                }
                return Ok(new { message = "Topic submitted successfully!" });
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("edit/{topicId}")]
        public async Task<IActionResult> EditTopic(int topicId, [FromBody] TopicUpdateDto request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
                await _topicService.EditTopicAsync(userId, topicId, request);

                var groupId = await _topicService.GetGroupIdByTopicIdAsync(topicId);
                if (groupId.HasValue)
                {
                    var mentorEmail = await _topicService.GetMentorEmailByGroupId(groupId.Value);
                    if (!string.IsNullOrEmpty(mentorEmail))
                    {
                        await _hubContext.Clients.User(mentorEmail).SendAsync("ReceiveNotification", new
                        {
                            type = "TOPIC_SUBMITTED", 
                            message = "A topic has been updated."
                        });
                    }
                }
                return Ok(new { message = "Topic updated successfully." });
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetTopicByGroupId(int groupId)
        {
            try
            {
                var topic = await _topicService.GetTopicByGroupIdAsync(groupId);
                return Ok(topic);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("mentor/all-topics")]
        [Authorize(Roles = "Mentor")]
        public async Task<IActionResult> GetPendingTopics()
        { 
            var mentorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var topics = await _topicService.GetAllTopicsForMentorAsync(mentorId);

            return Ok(topics);
        }

        [HttpGet("mentor-board")]
        public async Task<IActionResult> GetMentorBoard()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { Message = "Invalid token: User ID not found." });
            }

            int mentorId = int.Parse(userIdClaim.Value);

            var response = await _topicService.GetMentorProposalBoardAsync(mentorId);

            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}