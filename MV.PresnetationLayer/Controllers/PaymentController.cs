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
            if (request == null || request.Amount <= 0 || string.IsNullOrEmpty(request.OrderType))
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
            // Có thể lưu log giao dịch tại đây
            // Nếu muốn redirect về FE, có thể trả về Redirect(url)
            if (response.Success)
            {
                _vnPayService.SavePaymentOnline(response, invoiceId);
            }
            return Ok(response);
        }
    }
} 