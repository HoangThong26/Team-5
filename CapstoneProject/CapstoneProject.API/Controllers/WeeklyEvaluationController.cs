using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IService;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneProject.API.Controllers
{
    //[Authorize(Roles = "Mentor")]
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
        public async Task<IActionResult> EvaluateReport([FromBody] EvaluationRequest request)
        {
            try
            {
                // Assuming you retrieve MentorId from the authenticated User Claims
                int mentorId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                //int mentorId = 87;
                string resultMessage = await _evaluationService.EvaluateAsync(mentorId, request);

                return Ok(new
                {
                    success = true,
                    message = resultMessage
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
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

        [HttpGet("pending-reports")]
        public async Task<IActionResult> GetPendingReports()
        {
            try
            {
                var reports = await _evaluationService.GetPendingReportsAsync();

                if (reports == null || !reports.Any())
                {
                    return NotFound(new { message = "No pending reports found." });
                }

                return Ok(reports);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
