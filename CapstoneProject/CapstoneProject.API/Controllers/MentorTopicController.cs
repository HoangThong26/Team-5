using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CapstoneProject.Application.Interface.IService;
using CapstoneProject.Application.DTO;
using System.Security.Claims;

namespace CapstoneProject.API.Controllers
{
    [Route("api/mentor/topics")]
    [ApiController]
    [Authorize(Roles = "Mentor")] // Chỉ Mentor mới được gọi các API này
    public class MentorTopicController : ControllerBase
    {
        private readonly ITopicService _topicService;

        public MentorTopicController(ITopicService topicService)
        {
            _topicService = topicService;
        }

        [HttpPost("approve")]
        public async Task<IActionResult> ApproveTopic([FromBody] TopicApprovalRequest request)
        {
            try
            {
                var mentorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                await _topicService.ApproveTopicAsync(mentorId, request);

                return Ok(new { message = $"Topic status has been updated: {request.Status}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}