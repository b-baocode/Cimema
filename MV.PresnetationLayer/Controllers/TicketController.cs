using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.ServiceInterfaces;

namespace MV.PresnetationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly ITicketInvoiceService _ticketInvoiceService;

        public TicketController(ITicketInvoiceService ticketInvoiceService)
        {
            _ticketInvoiceService = ticketInvoiceService;
        }

        /// <summary>
        /// Lấy danh sách vé theo UserId
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Danh sách vé với QR code</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<TicketResponse>>> GetTicketsByUserId(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("UserId is required.");
                }

                var tickets = await _ticketInvoiceService.GetTicketsByUserIdAsync(userId);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Check vé (cập nhật status thành "Checked")
        /// </summary>
        /// <param name="ticketId">ID của vé</param>
        /// <returns>Kết quả check vé</returns>
        [HttpPut("{ticketId}/check")]
        public async Task<ActionResult<bool>> CheckTicket(int ticketId)
        {
            try
            {
                if (ticketId <= 0)
                {
                    return BadRequest("Invalid ticket ID.");
                }

                var result = await _ticketInvoiceService.CheckTicketAsync(ticketId);

                if (result)
                {
                    return Ok(new { success = true, message = "Ticket checked successfully." });
                }
                else
                {
                    return NotFound(new { success = false, message = "Ticket not found." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo QR code cho vé (test API)
        /// </summary>
        /// <param name="ticketId">ID của vé</param>
        /// <returns>QR code base64 string</returns>
        [HttpGet("{ticketId}/qr-code")]
        public async Task<ActionResult<string>> GetTicketQrCode(int ticketId)
        {
            try
            {
                if (ticketId <= 0)
                {
                    return BadRequest("Invalid ticket ID");
                }

                var qrCode = await _ticketInvoiceService.GenerateTicketQrCodeAsync(ticketId);
                return Ok(new { qrCode = qrCode });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy chi tiết vé theo InvoiceId
        /// </summary>
        [HttpGet("{invoiceId}")]
        public async Task<ActionResult<TicketDetailFullResponse>> GetTicketDetailByInvoiceId(int invoiceId)
        {
            if (invoiceId <= 0)
                return BadRequest("Invalid invoiceId.");
            var result = await _ticketInvoiceService.GetTicketDetailByInvoiceIdAsync(invoiceId);
            if (result == null)
                return NotFound();
            return Ok(result);
        }
    }
}