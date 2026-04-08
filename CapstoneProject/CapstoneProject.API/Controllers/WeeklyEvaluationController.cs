using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneProject.API.Controllers
{
    [Authorize(Roles = "Mentor")]
    [Route("api/[controller]")]
    [ApiController]
    public class WeeklyEvaluationController : ControllerBase
    {
        private readonly IWeeklyEvaluationService _evaluationService;

        public WeeklyEvaluationController(IWeeklyEvaluationService evaluationService)
        {
            _evaluationService = evaluationService;
        }

        [HttpPost("submit-grade")]
        public async Task<IActionResult> Evaluate([FromBody] EvaluationRequest request)
        {
            try
            {
                var mentorIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(mentorIdClaim)) return Unauthorized();

                await _evaluationService.EvaluateAsync(int.Parse(mentorIdClaim), request);

                return Ok(new { message = "Evaluation submitted successfully!" });
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("report/{reportId}")]
        public async Task<IActionResult> GetEvaluation(int reportId)
        {
            var evaluation = await _evaluationService.GetEvaluationByReportIdAsync(reportId);

            if (evaluation == null)
            {
                return NotFound(new { message = "No evaluation found for this report yet." });
            }

            return Ok(evaluation);
        }
    }
}
