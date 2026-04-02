using CapstoneProject.Application.DTOs;
using CapstoneProject.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticService _statisticService;

        public StatisticsController(IStatisticService statisticService)
        {
            _statisticService = statisticService;
        }

        /// <summary>
        /// Lấy thống kê Pass/Fail (US-29) hỗ trợ lọc theo Semester và Major (US-32)
        /// </summary>
        /// <param name="semesterId">ID của kỳ học (Tùy chọn)</param>
        /// <param name="majorId">ID của chuyên ngành (Tùy chọn)</param>
        /// <returns>PassFailStatisticDto</returns>
        [HttpGet("pass-fail")]
        public async Task<ActionResult<PassFailStatisticDto>> GetPassFailChartData(
            [FromQuery] int? semesterId,
            [FromQuery] int? majorId)
        {
            try
            {
                var data = await _statisticService.GetPassFailStatisticsAsync(semesterId, majorId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                // Thay bằng Logger thực tế của bạn
                return StatusCode(500, new { message = "Lỗi khi lấy dữ liệu thống kê", error = ex.Message });
            }
        }
    }
}