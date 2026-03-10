using CapstoneProject.API.Hubs;
using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

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
    }
}