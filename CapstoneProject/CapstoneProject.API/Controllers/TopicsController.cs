using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneProject.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TopicsController : ControllerBase
    {
        private readonly ITopicService _topicService;

        public TopicsController(ITopicService topicService)
        {
            _topicService = topicService;
        }

        [HttpPost("submit")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitTopic(TopicSubmitRequest request)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            await _topicService.SubmitTopicAsync(userId, request);

            return Ok(new { message = "Topic submitted successfully" });
        }


        [HttpPut("approve")]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<IActionResult> ApproveTopic(TopicApprovalRequest request)
        {
            var reviewerId = int.Parse(
                User.FindFirst("UserId")!.Value);

            await _topicService.ApproveTopicAsync(reviewerId, request);

            return Ok("Topic reviewed successfully");
        }
    }
}
