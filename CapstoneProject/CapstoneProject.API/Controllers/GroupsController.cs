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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = int.Parse(User.FindFirst("id")?.Value);

            var result = await _groupService.CreateGroupAsync(userIdClaim, request);

            // Updated comparison string to English
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

            int currentUserId = 1;

            var result = await _groupService.InviteMemberAsync(currentUserId, request);

            // Updated check to English "Success"
            if (result.StartsWith("Success"))
            {
                return Ok(new { message = result });
            }

            return BadRequest(new { message = result });
        }

        [HttpGet("accept-invite")]
        public async Task<IActionResult> AcceptInvite([FromQuery] int invitationId)
        {
            var result = await _groupService.AcceptInviteAsync(invitationId);

            // Updated check and HTML content to English
            if (result.Contains("successfully"))
            {
                string htmlSuccess = $@"
                    <div style='text-align:center; padding:50px; font-family:Arial;'>
                        <h1 style='color:green;'>Success!</h1>
                        <p>{result}</p>
                        <p>You can close this tab and return to the system.</p>
                    </div>";
                return Content(htmlSuccess, "text/html");
            }

            string htmlError = $@"
                    <div style='text-align:center; padding:50px; font-family:Arial;'>
                        <h1 style='color:red;'>Oops!</h1>
                        <p>{result}</p>
                    </div>";
            return Content(htmlError, "text/html");
        }
    }
}