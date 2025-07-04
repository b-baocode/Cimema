using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.ServiceInterfaces;

namespace MV.PresnetationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckInController : ControllerBase
    {
        private readonly ICheckInService _checkInService;
        private readonly IQrCodeService _qrCodeService;

        public CheckInController(ICheckInService checkInService, IQrCodeService qrCodeService)
        {
            _checkInService = checkInService;
            _qrCodeService = qrCodeService;
        }

        /// <summary>
        /// Check-in vé bằng QR code
        /// </summary>
        [HttpPost("checkin-by-qr")]
        public async Task<IActionResult> CheckInByQrCode([FromBody] CheckInQrRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.QrContent))
                    return BadRequest("QR content không được để trống");

                var result = await _checkInService.CheckInByQrCodeAsync(request.QrContent);
                return Ok(new
                {
                    success = true,
                    message = "Check-in thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Check-in vé bằng booking ID
        /// </summary>
        [HttpPost("checkin-by-booking/{bookingId}")]
        public async Task<IActionResult> CheckInByBookingId(int bookingId)
        {
            try
            {
                var result = await _checkInService.CheckInByBookingIdAsync(bookingId);
                return Ok(new
                {
                    success = true,
                    message = "Check-in thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy thông tin vé để in bill (không check-in)
        /// </summary>
        [HttpGet("ticket-info/{bookingId}")]
        public async Task<IActionResult> GetTicketInfo(int bookingId)
        {
            try
            {
                var result = await _checkInService.GetTicketInfoForPrintingAsync(bookingId);
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Tạo QR code cho booking
        /// </summary>
        [HttpGet("generate-qr/{bookingId}")]
        public async Task<IActionResult> GenerateQrCode(int bookingId)
        {
            try
            {
                var qrCode = await _qrCodeService.GenerateQrCodeAsync(bookingId);
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        qrCode = qrCode,
                        bookingId = bookingId
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Giải mã QR code để lấy booking ID
        /// </summary>
        [HttpPost("decode-qr")]
        public async Task<IActionResult> DecodeQrCode([FromBody] CheckInQrRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.QrContent))
                    return BadRequest("QR content không được để trống");

                var bookingId = _qrCodeService.DecodeBookingIdFromQrCode(request.QrContent);
                if (!bookingId.HasValue)
                    return BadRequest("QR code không hợp lệ");

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        bookingId = bookingId.Value
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }

    public class CheckInQrRequest
    {
        public string QrContent { get; set; } = string.Empty;
    }
} 