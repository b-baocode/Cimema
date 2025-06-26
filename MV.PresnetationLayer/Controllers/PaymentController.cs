using Microsoft.AspNetCore.Mvc;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.ServiceInterfaces;

namespace MV.PresnetationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly ITicketInvoiceService _ticketInvoiceService;
        public PaymentController(IVnPayService vnPayService, ITicketInvoiceService ticketInvoiceService)
        {
            _vnPayService = vnPayService;
            _ticketInvoiceService = ticketInvoiceService;
        }

        /// <summary>
        /// Tạo URL thanh toán VnPay
        /// </summary>
        [HttpPost("create-vnpay-url")]
        public async Task<IActionResult> CreateVnPayUrl([FromBody] PaymentInformationRequest request)
        {
            if (request == null || request.InvoiceId <= 0)
                return BadRequest("Invalid payment request");
            var invoice = await _ticketInvoiceService.GetByIdAsync(request.InvoiceId);
            if (invoice == null)
                return BadRequest("Invoice not found");
            if (invoice.ScoreDiscountAmount == null || invoice.ScoreDiscountAmount <= 0)
                return BadRequest("Invalid invoice amount");
            var url = _vnPayService.CreatePaymentUrl(request, (double)invoice.ScoreDiscountAmount.Value, HttpContext);
            return Ok(new { paymentUrl = url });
        }

        /// <summary>
        /// Nhận callback từ VnPay
        /// </summary>
        [HttpGet("vnpay-callback")]
        public IActionResult VnPayCallback([FromQuery] int? invoiceId = null)
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            
            // Lưu tất cả các trường hợp thanh toán để tracking
            _vnPayService.SavePaymentOnline(response, invoiceId);
            
            // Trả về thông tin chi tiết về kết quả thanh toán
            var result = new
            {
                success = response.Success,
                responseCode = response.VnPayResponseCode,
                message = GetPaymentMessage(response.VnPayResponseCode),
                orderId = response.OrderId,
                transactionId = response.TransactionId,
                amount = response.OrderDescription
            };
            
            return Ok(result);
        }

        private string GetPaymentMessage(string vnPayResponseCode)
        {
            return vnPayResponseCode switch
            {
                "00" => "Thanh toán thành công",
                "24" => "Khách hàng hủy giao dịch",
                "INVALID_SIGNATURE" => "Chữ ký không hợp lệ",
                _ => $"Giao dịch thất bại (Mã lỗi: {vnPayResponseCode})"
            };
        }
    }
} 