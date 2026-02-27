using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IService;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CapstoneProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupService _groupService;

        public GroupsController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
        {
            // Dữ liệu hợp lệ theo DataAnnotations chưa? (Ví dụ: tên nhóm rỗng, số người > 5)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Giả lập cứng UserId = 1 (Tạm thời test. Sau này lấy ra từ JWT Token)
            int currentUserId = 1;

            var result = await _groupService.CreateGroupAsync(currentUserId, request);

            if (result == "Tạo nhóm thành công!")
            {
                return Ok(new { message = result });
            }

            // Trả về lỗi nếu đã có nhóm hoặc lỗi hệ thống
            return BadRequest(new { message = result });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroupDetails(int id)
        {
            var result = await _groupService.GetGroupDetailsAsync(id);

            // Nếu không tìm thấy nhóm (có thể do nhập sai ID hoặc nhóm đã bị xóa)
            if (result == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin nhóm!" });
            }

            return Ok(result);
        }
        [HttpPost("invite")]
        // [Authorize] <-- Tạm comment đợi làm chức năng Login
        public async Task<IActionResult> InviteMember([FromBody] InviteMemberRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Giả lập Leader đang đăng nhập có UserId = 1
            int currentUserId = 1;

            var result = await _groupService.InviteMemberAsync(currentUserId, request);

            if (result.StartsWith("Thành công"))
            {
                return Ok(new { message = result });
            }

            return BadRequest(new { message = result });
        }

        // THÊM API NÀY VÀO GROUPS CONTROLLER
        [HttpGet("accept-invite")]
        public async Task<IActionResult> AcceptInvite([FromQuery] int invitationId)
        {
            var result = await _groupService.AcceptInviteAsync(invitationId);

            // Tùy chỉnh giao diện trả về cho người dùng khi click link
            if (result.Contains("thành công"))
            {
                // Trả về một mã HTML đơn giản báo thành công
                string htmlSuccess = $@"
                    <div style='text-align:center; padding:50px; font-family:Arial;'>
                        <h1 style='color:green;'>Thành công!</h1>
                        <p>{result}</p>
                        <p>Bạn có thể đóng tab này và quay lại hệ thống.</p>
                    </div>";
                return Content(htmlSuccess, "text/html");
            }

            // Báo lỗi bằng giao diện HTML
            string htmlError = $@"
                    <div style='text-align:center; padding:50px; font-family:Arial;'>
                        <h1 style='color:red;'>Rất tiếc!</h1>
                        <p>{result}</p>
                    </div>";
            return Content(htmlError, "text/html");
        }
    }
}