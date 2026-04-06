using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WeeklyReportController : ControllerBase
{
    private readonly IWeeklyReportService _service;
    public WeeklyReportController(IWeeklyReportService service) => _service = service;

    [Authorize]
    [HttpPost("submit")]
    public async Task<IActionResult> SubmitWeekly([FromForm] WeeklyReportRequest request)
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized(new { message = "Session expired." });

            var result = await _service.SubmitReportAsync(int.Parse(userIdStr), request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpGet("group/{groupId}")]
    public async Task<IActionResult> GetGroupReports(int groupId)
    {
        var reports = await _service.GetReportsByGroupIdAsync(groupId);
        return Ok(reports);
    }

    [Authorize(Roles = "Mentor")]
    [HttpGet("mentor-inbox")]

    public async Task<IActionResult> GetMentorInbox()
    {
        try
        {
            // Lấy ID của Mentor đang đăng nhập từ JWT Token
            var mentorIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(mentorIdStr)) return Unauthorized();

            var reports = await _service.GetReportsForMentorAsync(int.Parse(mentorIdStr));

            // Trả về danh sách bài nộp cho Mentor
            return Ok(reports);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("history/{groupId}")]
    public async Task<IActionResult> GetHistory(int groupId)
    {
        try
        {
            var history = await _service.GetGroupHistoryAsync(groupId);

            if (history == null || history.Count == 0)
            {
                return Ok(new List<WeeklyReportHistoryDto>());
            }

            return Ok(history);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error history " + ex.Message });
        }
    }
    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateReport(int id, [FromForm] WeeklyReportRequest request)
    {
        var response = await _service.UpdateWeeklyReportAsync(id, request);

        if (!response.Success)
        {
            return BadRequest(new { message = response.Message });
        }

        return Ok(new { message = response.Message });
    }

    [HttpGet("download/{fileName}")]
    public async Task<IActionResult> DownloadFile(string fileName)
    {
        try
        {
            var (fileContent, contentType, name) = await _service.DownloadReportAsync(fileName);
            return File(fileContent, contentType, name);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Lỗi khi tải file: " + ex.Message });
        }
    }
}