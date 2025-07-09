using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel.DashBoardRequest;
using MV.ApplicationLayer.DTO.ResponseModel.DashBoardResponse;
using MV.ApplicationLayer.ServiceInterfaces;

namespace MV.PresnetationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue(
            [FromQuery] string? period = "today",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? year = null,
            [FromQuery] int? month = null,
            [FromQuery] int? day = null)
        {
            try
            {
                // Validate period parameter
                if (!string.IsNullOrEmpty(period) && 
                    !new[] { "today", "month", "year", "custom" }.Contains(period.ToLower()))
                {
                    return BadRequest(new { message = "Invalid period. Must be 'today', 'month', 'year', or 'custom'" });
                }

                // Validate date range if provided
                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                {
                    return BadRequest(new { message = "Start date must be before end date" });
                }

                // Validate year, month, day parameters
                if (year.HasValue && (year.Value < 1900 || year.Value > 2100))
                {
                    return BadRequest(new { message = "Invalid year. Must be between 1900 and 2100" });
                }

                if (month.HasValue && (month.Value < 1 || month.Value > 12))
                {
                    return BadRequest(new { message = "Invalid month. Must be between 1 and 12" });
                }

                if (day.HasValue && (day.Value < 1 || day.Value > 31))
                {
                    return BadRequest(new { message = "Invalid day. Must be between 1 and 31" });
                }

                var result = await _dashboardService.GetRevenueAsync(period, startDate, endDate, year, month, day);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("revenue/chart")]
        public async Task<IActionResult> GetRevenueChart([FromBody] RevenueChartRequest request)
        {
            if (!new[] { "day", "month", "year" }.Contains(request.Type))
                return BadRequest(new { message = "type must be 'day', 'month', or 'year'" });
            if (request.StartDate > request.EndDate)
                return BadRequest(new { message = "startDate must be before endDate" });

            var result = await _dashboardService.GetRevenueChartAsync(request);
            return Ok(result);
        }
    }
} 