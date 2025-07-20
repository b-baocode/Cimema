using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.ServiceInterfaces;

namespace MV.PresnetationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RefundController : ControllerBase
    {
        private readonly IRefundService _refundService;

        public RefundController(IRefundService refundService)
        {
            _refundService = refundService;
        }

        /// <summary>
        /// Yêu cầu hoàn tiền (User)
        /// </summary>
        [HttpPost("request")]
        public async Task<ActionResult<RefundResponse>> RequestRefund([FromBody] RefundRequest request)
        {
            try
            {
                var result = await _refundService.RequestRefundAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Xử lý hoàn tiền (Admin/Manager)
        /// </summary>
        [HttpPost("process/{invoiceId}")]
        public async Task<ActionResult<RefundResponse>> ProcessRefund(int invoiceId, [FromBody] ProcessRefundRequest request)
        {
            try
            {
                var result = await _refundService.ProcessRefundAsync(invoiceId, request.RefundReason, request.AdminId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Tính toán số tiền hoàn tiền có thể nhận
        /// </summary>
        [HttpGet("calculate/{invoiceId}")]
        public async Task<ActionResult<decimal>> CalculateRefundAmount(int invoiceId)
        {
            try
            {
                var amount = await _refundService.CalculateRefundAmountAsync(invoiceId);
                return Ok(new { refundAmount = amount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Kiểm tra điều kiện hoàn tiền
        /// </summary>
        [HttpGet("check-eligibility/{invoiceId}/{userId}")]
        public async Task<ActionResult<bool>> CheckRefundEligibility(int invoiceId, string userId)
        {
            try
            {
                var isEligible = await _refundService.CheckRefundEligibilityAsync(invoiceId, userId);
                return Ok(new { isEligible });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy lịch sử hoàn tiền của user
        /// </summary>
        [HttpGet("history/{userId}")]
        public async Task<ActionResult<List<RefundResponse>>> GetRefundHistory(string userId)
        {
            try
            {
                var history = await _refundService.GetRefundHistoryAsync(userId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin hoàn tiền theo invoice ID
        /// </summary>
        [HttpGet("invoice/{invoiceId}")]
        public async Task<ActionResult<RefundResponse>> GetRefundByInvoiceId(int invoiceId)
        {
            try
            {
                var refund = await _refundService.GetRefundByInvoiceIdAsync(invoiceId);
                return Ok(refund);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class ProcessRefundRequest
    {
        public string RefundReason { get; set; } = string.Empty;
        public string AdminId { get; set; } = string.Empty;
    }
} 