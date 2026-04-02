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
        /// <summary>
        /// Lấy thống kê phân bổ dải điểm (US-30) hỗ trợ lọc theo Semester và Major (US-32)
        /// </summary>
        /// <param name="semesterId">ID của kỳ học (Tùy chọn)</param>
        /// <param name="majorId">ID của chuyên ngành (Tùy chọn)</param>
        /// <returns>Danh sách dải điểm và số lượng</returns>
        [HttpGet("grade-distribution")]
        public async Task<ActionResult<List<GradeDistributionDto>>> GetGradeDistributionChartData(
            [FromQuery] int? semesterId,
            [FromQuery] int? majorId)
        {
            try
            {
                var data = await _statisticService.GetGradeDistributionAsync(semesterId, majorId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi thực tế ở đây nếu hệ thống bạn có ILogger
                return StatusCode(500, new { message = "Lỗi khi lấy dữ liệu thống kê dải điểm", error = ex.Message });
            }
        }
    }
}