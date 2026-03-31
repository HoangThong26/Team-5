using CapstoneProject.Application.Interface.IService;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class GradesController : ControllerBase
{
    private readonly IGradeService _gradeService;

    public GradesController(IGradeService gradeService)
    {
        _gradeService = gradeService;
    }

    [HttpPost("calculate/{groupId}")]
    public async Task<IActionResult> CalculateFinalGrade(int groupId)
    {
        try
        {
            var result = await _gradeService.CalculateAndSaveFinalGrade(groupId);
            return Ok(new { Message = "Final grade calculated successfully", Score = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}