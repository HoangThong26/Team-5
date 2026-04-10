using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CapstoneProject.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] AdminCreateUserRequest request)
        {
            try
            {
                var result = await _adminService.CreateUserByRoleAsync(request);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("id")?.Value;

            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized();
            }

            var users = await _adminService.GetAllUsersAsync(currentUserId);

            return Ok(users);
        }

        [HttpDelete("delete-user/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                await _adminService.DeleteAsync(userId);
                return Ok(new { message = "User deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("unlock-account")]
        public async Task UnlockAcountAsync(int userId)
        {
            await _adminService.UnlockAccountAsync(userId);
        }

        [HttpGet("search-users")]
        public async Task<IActionResult> SearchUsers([FromQuery] string keyword)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("id")?.Value;

            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized();
            }

            var users = await _adminService.SearchUsersAsync(keyword, currentUserId);

            return Ok(users);
        }


        [HttpPost("import-users")]
        public async Task<IActionResult> ImportUsersFromExcel(IFormFile file)
        {
            if (file == null)
            {
                return BadRequest(new { Message = "No file was uploaded. Please provide an Excel file." });
            }

            if (file.Length == 0)
            {
                return BadRequest(new { Message = "The uploaded file is empty." });
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xls" && extension != ".xlsx")
            {
                return BadRequest(new { Message = "Invalid file format. Please upload a valid Excel file (.xls or .xlsx)." });
            }

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    int importedCount = await _adminService.ImportUsersFromExcelAsync(stream);
                    if (importedCount == 0)
                    {
                        return Ok(new
                        {
                            Message = "No new users were imported. The data might be empty or all emails already exist in the system.",
                            ImportedCount = 0
                        });
                    }
                    return Ok(new
                    {
                        Message = $"Successfully imported {importedCount} user(s).",
                        ImportedCount = importedCount
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "An error occurred while processing the Excel file.",
                    Details = ex.Message
                });
            }
        }
        [HttpGet("export-students")]
        public async Task<IActionResult> ExportStudents()
        {
            try
            {
                var fileBytes = await _adminService.ExportStudentsToExcelAsync();

                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string fileName = $"StudentList{DateTime.Now:ddMMyyyy}.xlsx";
                return File(fileBytes, contentType, fileName);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { Message = "Errol Export file Excel.", Error = ex.Message });
            }
        }

        [HttpPost("setup-timeline")]
        public async Task<IActionResult> SetupTimeline([FromBody] AdminSetupRequest request)
        {
            try
            {
                if (request.StartDate == default)
                    return BadRequest(new { message = "Invalid start date." });

                await _adminService.SetupTimelineAsync(request.StartDate);

                return Ok(new { message = "Project timeline has been initialized successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("admin/all-mentors")]
        [Authorize(Roles = "Admin")] // Comment lại nếu đang test
        public async Task<IActionResult> GetAllMentors()
        {
            try
            {
                var result = await _adminService.GetAllMentorsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}

