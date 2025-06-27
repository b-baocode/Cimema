using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel.BookingRequest;
using MV.ApplicationLayer.ServiceInterfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MV.PresnetationLayer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            request.UserId = userId; // Overwrite for security
            var result = await _bookingService.CreateBookingAsync(request);
            return Ok(result);
        }

        [HttpGet("{invoiceId}")]
        public async Task<IActionResult> GetBookingById(int invoiceId)
        {
            var result = await _bookingService.GetBookingByIdAsync(invoiceId);
            return Ok(result);
        }

        [HttpPut("cancel/{invoiceId}")]
        public async Task<IActionResult> CancelBooking(int invoiceId)
        {
            try
            {
                var result = await _bookingService.CancelBookingAsync(invoiceId);
                if (result)
                {
                    return Ok(new { message = "Đơn hàng đã được hủy thành công. Ghế đã được giải phóng." });
                }
                else
                {
                    return BadRequest(new { message = "Không thể hủy đơn hàng. Có thể đơn hàng đã được hủy trước đó." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetBookingsByUserId(string userId)
        {
            var result = await _bookingService.GetBookingsByUserAsync(userId);
            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllBookings()
        {
            var result = await _bookingService.GetAllBookingsAsync();
            return Ok(result);
        }

      
    }
} 