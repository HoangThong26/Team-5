using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> GetUsers()
        {
            var users = await _adminService.GetAllUsersAsync();
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
        public async Task<IActionResult> SearchUsers(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return BadRequest("Keyword is required");

            var result = await _adminService.SearchUsersAsync(keyword);

            return Ok(result);
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
        }
}
