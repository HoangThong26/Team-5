using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CapstoneProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = int.Parse(User.FindFirst("id")?.Value);
            //var userIdClaim = 5;
            var result = await _groupService.CreateGroupAsync(userIdClaim, request);

            if (result == "Group created successfully!")
            {
                return Ok(new { message = result });
            }

            return BadRequest(new { message = result });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroupDetails(int id)
        {
            var result = await _groupService.GetGroupDetailsAsync(id);

            if (result == null)
            {
                return NotFound(new { message = "Group information not found!" });
            }

            return Ok(result);
        }

        [HttpPost("invite")]
        public async Task<IActionResult> InviteMember([FromBody] InviteMemberRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            int currentUserId = int.Parse(User.FindFirst("id")?.Value);
            //int currentUserId = 5;

            var result = await _groupService.InviteMemberAsync(currentUserId, request);
            if (result.StartsWith("Success"))
            {
                return Ok(new { message = result });
            }

            return BadRequest(new { message = result });
        }

        [HttpGet("accept-invite")]
        [AllowAnonymous] // Cho phép nhấn từ Email mà không cần login ngay
        public async Task<IActionResult> AcceptInvite([FromQuery] int invitationId)
        {
            // Service này sẽ gọi Repository có chứa Transaction (Thêm member + Check 4 người + Gán Mentor)
            var result = await _groupService.AcceptInviteAsync(invitationId);

            // logic hiển thị HTML dựa trên kết quả trả về từ logic "đủ 4 người"
            if (result.Contains("successfully"))
            {
                string htmlSuccess = $@"
                <div style='text-align:center; padding:50px; font-family:Arial;'>
                    <h1 style='color:green;'>Success!</h1>
                    <p style='font-size:18px;'>{result}</p>
                    <p>You can close this tab and return to the system.</p>
                </div>";
                return Content(htmlSuccess, "text/html");
            }

            string htmlError = $@"
                <div style='text-align:center; padding:50px; font-family:Arial;'>
                    <h1 style='color:red;'>Oops!</h1>
                    <p style='font-size:18px;'>{result}</p>
                </div>";
            return Content(htmlError, "text/html");
        }
    }
}