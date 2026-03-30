using Microsoft.AspNetCore.Mvc;
using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CapstoneProject.API.Controllers
{
    [Route("api/topics-search")]
    [ApiController]
    public class TopicSearchController : ControllerBase
    {
        private readonly ITopicSearchService _topicSearchService;

        public TopicSearchController(ITopicSearchService topicSearchService)
        {
            _topicSearchService = topicSearchService;
        }

        // ==========================================
        // 1. API DÀNH CHO ADMIN (Xem toàn bộ)
        // ==========================================
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SearchGlobal([FromQuery] TopicSearchRequest request)
        {
            var result = await _topicSearchService.SearchGlobalAsync(request);
            return Ok(result);
        }

        // ==========================================
        // 2. API DÀNH CHO MENTOR (Chỉ xem nhóm mình)
        // ==========================================
        [HttpGet("mentor")]
        [Authorize(Roles = "Mentor")]
        public async Task<IActionResult> SearchForMentor([FromQuery] TopicSearchRequest request)
        {
            // Bóc tách ID của Mentor từ Token một cách tự động và bảo mật
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("id")?.Value
                           ?? User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentMentorId))
            {
                return Unauthorized(new { message = "Không xác định được danh tính Mentor từ Token. Vui lòng đăng nhập lại." });
            }

            // Truyền ID lấy từ Token xuống Service
            var result = await _topicSearchService.SearchForMentorAsync(request, currentMentorId);

            return Ok(result);
        }
    }
}