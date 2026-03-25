using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneProject.API.Controllers
{
    [Route("api/topics-search")] // Tạo đường dẫn API riêng không đụng chạm đến API cũ
    [ApiController]
    public class TopicSearchController : ControllerBase
    {
        private readonly ITopicSearchService _searchService;

        public TopicSearchController(ITopicSearchService searchService)
        {
            _searchService = searchService;
        }

        // Endpoint này dành riêng cho chức năng Search Global của Admin
        [HttpGet("admin")]
        // [Authorize(Roles = "Admin")] // Bạn có thể mở comment dòng này sau khi test FE thành công để bảo mật
        public async Task<IActionResult> SearchGlobal([FromQuery] TopicSearchRequest request)
        {
            try
            {
                var result = await _searchService.SearchGlobalAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi hệ thống khi tìm kiếm", Details = ex.Message });
            }
        }
    }
}