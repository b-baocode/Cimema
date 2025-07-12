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
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Nếu user hiện tại là Admin, Manager, Employee hoặc Staff, cho phép đặt vé cho user khác
            if (User.IsInRole("Admin") || User.IsInRole("Manager") || User.IsInRole("Employee") || User.IsInRole("Staff"))
            {
                // Cho phép đặt vé cho user khác, nhưng vẫn kiểm tra userId có hợp lệ
                if (string.IsNullOrEmpty(request.UserId))
                {
                    return BadRequest("UserId is required when booking for another user");
                }
            }
            else
            {
                // Nếu là Customer, chỉ được đặt vé cho chính mình
                request.UserId = currentUserId;
            }

            var result = await _bookingService.CreateBookingAsync(request);
            return Ok(result);
        }

        [HttpGet("{invoiceId}")]
        public async Task<IActionResult> GetBookingById(int invoiceId)
        {
            var result = await _bookingService.GetBookingByIdAsync(invoiceId);
            return Ok(result);
        }

    
      

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetBookingsByUserId(string userId)
        {
            var result = await _bookingService.GetBookingsByUserAndStatusAsync(userId, "Success");
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