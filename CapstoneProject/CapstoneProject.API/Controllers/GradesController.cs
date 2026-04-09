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

    // Endpoint dành cho Admin: Lấy toàn bộ danh sách để xem và Publish
    [HttpGet("all")]
    public async Task<IActionResult> GetAllGrades()
    {
        var grades = await _gradeService.GetAllFinalGrades();
        return Ok(grades);
    }

    // Endpoint dành cho Admin: Thực hiện công bố điểm (US-24 AC1)
    [HttpPost("publish/{groupId}")]
    public async Task<IActionResult> PublishGrade(int groupId)
    {
        await _gradeService.PublishGrade(groupId);
        return Ok(new { message = "Grade published successfully!" });
    }

    // Endpoint dành cho Sinh viên: Xem điểm của nhóm mình (US-24 AC2 & AC3)
    [HttpGet("my-grade/{groupId}")]
    public async Task<IActionResult> GetMyGrade(int groupId)
    {
        var grade = await _gradeService.GetGradeForStudent(groupId);
        if (grade == null)
        {
            return NotFound(new { message = "No grade record found for this group." });
        }
        return Ok(grade);
    }
}