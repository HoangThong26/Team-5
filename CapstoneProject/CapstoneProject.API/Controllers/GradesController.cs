using CapstoneProject.Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class GradesController : ControllerBase
{
    private readonly IGradeService _gradeService;

    public GradesController(IGradeService gradeService)
    {
        _gradeService = gradeService;
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin,Mentor")]
    public async Task<IActionResult> GetAllGrades()
    {
        try
        {
            var result = await _gradeService.GetAllGradesAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("calculate/{groupId}")]
    [Authorize(Roles = "Admin")]
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

    [HttpPost("publish/{groupId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PublishGrade(int groupId)
    {
        try
        {
            await _gradeService.PublishGradeAsync(groupId);
            return Ok(new { Message = "Final grade published successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
