using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IService;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneProject.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
                // Kiểm tra AC3: Required fields
                if (request.PresentationScore < 0 || request.DemoScore < 0 || request.QAScore < 0)
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
    }
}