using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IService;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace CapstoneProject.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DefenseController : ControllerBase
    {
        private readonly IDefenseService _defenseService;

        public DefenseController(IDefenseService defenseService)
        {
            _defenseService = defenseService;
        }

        [HttpGet("my-registration-status")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyRegistrationStatus()
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdValue) || !int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized(new { message = "Unauthorized." });
            }

            var result = await _defenseService.GetMyRegistrationStatusAsync(userId);
            if (result == null)
            {
                return NotFound(new { message = "You are not in any group." });
            }

            return Ok(result);
        }

        [HttpPost("register")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> RegisterForDefense()
        {
            try
            {
                var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(userIdValue) || !int.TryParse(userIdValue, out var userId))
                {
                    return Unauthorized(new { message = "Unauthorized." });
                }

                await _defenseService.RegisterMyGroupAsync(userId);
                return Ok(new { message = "Defense registration saved successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("registrations")]
        public async Task<IActionResult> GetRegistrations()
        {
            var result = await _defenseService.GetDefenseRegistrationsAsync();
            return Ok(result);
        }

        [HttpGet("admin/committees")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCommittees()
        {
            var result = await _defenseService.GetDefenseCommitteesAsync();
            return Ok(result);
        }

        [HttpGet("admin/council-users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCouncilUsers()
        {
            var result = await _defenseService.GetCouncilUsersAsync();
            return Ok(result);
        }

        [HttpPost("admin/schedule")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateDefenseScheduleRequest request)
        {
            try
            {
                await _defenseService.CreateDefenseScheduleAsync(request);
                return Ok(new { message = "Defense schedule saved successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("admin/schedule/{defenseId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSchedule(int defenseId, [FromBody] UpdateDefenseScheduleRequest request)
        {
            try
            {
                await _defenseService.UpdateDefenseScheduleAsync(defenseId, request);
                return Ok(new { message = "Defense schedule updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("submit-evaluation")]
        public async Task<IActionResult> SubmitEvaluation([FromBody] DefenseEvaluationRequest request)
        {
            try
            {
                bool isInvalid = request.PresentationScore < 0 || request.PresentationScore > 10 ||
                         request.DemoScore < 0 || request.DemoScore > 10 ||
                         request.QAScore < 0 || request.QAScore > 10;
                // Kiểm tra AC3: Required fields
                if (isInvalid)
                {
                    return BadRequest(new { message = "Required evaluation fields missing or invalid." });
                }

                var result = await _defenseService.SubmitEvaluationAsync(request);
                return Ok(new { message = "Evaluation saved successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-evaluation/{defenseId}/{userId}")]
        public async Task<IActionResult> GetMyEvaluation(int defenseId, int userId)
        {
            var result = await _defenseService.GetMemberEvaluationAsync(defenseId, userId);
            if (result == null) return NotFound(new { message = "No evaluation found." });
            return Ok(result);
        }

        [HttpGet("assigned-defenses")]
        public async Task<IActionResult> GetAssignedDefenses()
        {
            // Lấy UserId từ Token (hỗ trợ cả NameIdentifier và claim "id")
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "User session invalid." });
            }

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { message = "Invalid User ID format." });
            }

            // Gọi qua Service thay vì gọi trực tiếp DB Context
            var assignedGroups = await _defenseService.GetAssignedDefensesAsync(userId);
            return Ok(assignedGroups);
        }

        [HttpGet("search")]
        [Authorize(Roles = "Council")]
        public async Task<IActionResult> SearchDefenseSchedules([FromQuery] string? keyword, [FromQuery] string? status)
        {
            var result = await _defenseService.SearchDefenseSchedulesAsync(keyword, status);
            if (result.Count == 0)
            {
                return Ok(new { message = "No results found", data = new List<DefenseScheduleDto>() });
            }
            return Ok(result);
        }
    }
}