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
        public PaymentController(IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        /// <summary>
        /// Tạo URL thanh toán VnPay
        /// </summary>
        [HttpPost("create-vnpay-url")]
        public IActionResult CreateVnPayUrl([FromBody] PaymentInformationRequest request)
        {
            if (request == null || request.Amount <= 0)
                return BadRequest("Invalid payment request");
            var url = _vnPayService.CreatePaymentUrl(request, HttpContext);
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