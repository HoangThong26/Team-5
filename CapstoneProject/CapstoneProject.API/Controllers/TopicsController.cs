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
                return Ok(new { message = "Topic submitted successfully. Pending approval." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("edit/{topicId}")]
        public async Task<IActionResult> EditTopic(int topicId, [FromBody] TopicUpdateDto request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
                await _topicService.EditTopicAsync(userId, topicId, request);
                return Ok(new { message = "Topic updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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

        [HttpGet("mentor/pending-topics")]
        [Authorize(Roles = "Mentor")]
        public async Task<IActionResult> GetPendingTopics()
        { 
            var mentorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var topics = await _topicService.GetPendingTopicsForMentorAsync(mentorId);

            return Ok(topics);
        }
    }
}