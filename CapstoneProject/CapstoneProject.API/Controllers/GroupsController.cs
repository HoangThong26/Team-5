using CapstoneProject.API.Hubs;
using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CapstoneProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupService _groupService;
        private readonly IHubContext<NotificationHub> _hubContext;
        public GroupsController(IGroupService groupService, IHubContext<NotificationHub> hubContext)
        {
            _groupService = groupService;
            _hubContext = hubContext;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = int.Parse(User.FindFirst("id")?.Value);
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
            var result = await _groupService.InviteMemberAsync(currentUserId, request);
            if (result.StartsWith("Success"))
            {
                return Ok(new { message = result });
            }

            return BadRequest(new { message = result });
        }

        [HttpGet("accept-invite")]
        [AllowAnonymous]
        public async Task<IActionResult> AcceptInvite([FromQuery] int invitationId)
        {

            var result = await _groupService.AcceptInviteAsync(invitationId);

            if (result.Contains("successfully"))
            {
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
                {
                    type = "MEMBER_ACCEPTED",
                    message = "A new member just joined the group!"
                });
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

        [HttpGet("reject-invite")]
        [AllowAnonymous]
        public async Task<IActionResult> RejectInvite([FromQuery] int invitationId)
        {
            var result = await _groupService.RejectInviteAsync(invitationId);

            string statusColor = result.Contains("successfully") ? "orange" : "red";
            string title = result.Contains("successfully") ? "Invitation Rejected" : "Oops!";

            string html = $@"
            <div style='text-align:center; padding:50px; font-family:Arial;'>
                <h1 style='color:{statusColor};'>{title}</h1>
                <p style='font-size:18px;'>{result}</p>
                <p>You can close this tab now.</p>
            </div>";
            return Content(html, "text/html");
        }

        [HttpGet]
        public async Task<IActionResult> GetMyGroup()
        {
            var userIdClaim = int.Parse(User.FindFirst("id")?.Value);
            var result = await _groupService.GetMyGroupAsync(userIdClaim);

            if (result == null)
            {
                return NotFound(new { message = "You are not in any group yet." });
            }

            return Ok(result);
        }

        [HttpDelete("{groupId}/members/{targetUserId}")]
        public async Task<IActionResult> KickMember(int groupId, int targetUserId)
        {
            try
            {

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int requesterId))
                {
                    return Unauthorized(new { message = "Unauthorized or invalid token." });
                }

                //int requesterId = 52;

                var resultMessage = await _groupService.KickMemberAsync(requesterId, groupId, targetUserId);

                return Ok(new { message = resultMessage });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/all-groups")]
        public async Task<IActionResult> GetAllGroups()
        {
            var result = await _groupService.GetAllGroupsForAdminAsync();
            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/{groupId}/kick-mentor")]
        public async Task<IActionResult> KickMentorByAdmin(int groupId)
        {
            try
            {
                var result = await _groupService.KickMentorByAdminAsync(groupId);

                if (result.Contains("successfully"))
                {
                    return Ok(new { message = result });
                }

                return BadRequest(new { message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"System error: {ex.Message}" });
            }
        }

        [HttpDelete("admin/{groupId}")]
        [Authorize(Roles = "Admin,Student")] // Cho phép cả 2 Role gọi API
        public async Task<IActionResult> DeleteGroup(int groupId)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role);

            if (userIdClaim == null || roleClaim == null) return Unauthorized();

            int currentUserId = int.Parse(userIdClaim.Value);
            string currentUserRole = roleClaim.Value;

            var result = await _groupService.DeleteGroupByAdminAsync(groupId, currentUserId, currentUserRole);


            if (result.Contains("not found")) return NotFound(new { message = result });
            if (result.Contains("Access denied")) return Forbid();
            if (result.Contains("successfully")) return Ok(new { message = result });

            return StatusCode(500, new { message = result });
        }

        [HttpPost("admin/assign-mentor")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignMentor([FromBody] AssignMentorDto request)
        {
            if (request == null || request.GroupId <= 0 || request.MentorId <= 0)
            {
                return BadRequest(new { message = "Invalid Group ID or Mentor ID." });
            }

            try
            {
                string result = await _groupService.AssignMentorAsync(request.GroupId, request.MentorId);

                if (result == "SUCCESS")
                {
                    return Ok(new
                    {
                        message = "Mentor assigned successfully!",
                        status = "Active"
                    });
                }

                if (result == "Group not found.")
                {
                    return NotFound(new { message = result });
                }

                return BadRequest(new { message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
    }
}