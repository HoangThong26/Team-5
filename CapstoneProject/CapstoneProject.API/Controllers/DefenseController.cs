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

        [HttpPost("submit-evaluation")]
        public async Task<IActionResult> SubmitEvaluation([FromBody] DefenseEvaluationRequest request)
        {
            try
            {
                if (request.PresentationScore == null || request.DemoScore == null || request.QAScore == null)
                {
                    return BadRequest(new { message = "Required evaluation fields missing." });
                }
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
    }
}