using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouncilController : ControllerBase
    {
        private readonly ICouncilService _councilService;

        public CouncilController(ICouncilService councilService)
        {
            _councilService = councilService;
        }
        [HttpGet("available-staffs")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAvailableStaffs()
        {
            var result = await _councilService.GetStaffsForCouncilAsync();
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("create-full")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCouncilFull([FromBody] CouncilCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ServiceResponse<int>
                {
                    Success = false,
                    Message = "Dữ liệu đầu vào không hợp lệ."
                });
            }

            var result = await _councilService.CreateCouncilFullAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(CreateCouncilFull), new { id = result.Data }, result);
        }

        [HttpGet("search-staff")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SearchStaff([FromQuery] string? query)
        {
            var result = await _councilService.SearchStaffForCouncilAsync(query);
            return Ok(result);
        }
    }
}