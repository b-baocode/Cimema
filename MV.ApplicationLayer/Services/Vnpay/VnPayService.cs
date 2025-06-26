using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.Library;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;
using MV.ApplicationLayer.RepositoryInterfaces;

namespace MV.ApplicationLayer.Services.Vnpay
{
    public class VnpayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly string TimeZoneID = "SE Asia Standard Time";
        private readonly IPaymentOnlineRepository _paymentOnlineRepository;
        private readonly ITicketInvoiceService _ticketInvoiceService;
        private readonly IScoreService _scoreService;

        public VnpayService(IConfiguration configuration, IPaymentOnlineRepository paymentOnlineRepository, ITicketInvoiceService ticketInvoiceService, IScoreService scoreService)
        {
            _configuration = configuration;
            _paymentOnlineRepository = paymentOnlineRepository;
            _ticketInvoiceService = ticketInvoiceService;
            _scoreService = scoreService;
        }

        public string CreatePaymentUrl(PaymentInformationRequest model, double amount, HttpContext context)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary();
            var urlCallBack = $"{_configuration["PaymentCallBack:ReturnUrl"]}?invoiceId={model.InvoiceId}";

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((int)amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"{model.OrderDescription} {amount}");
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);

            var paymentUrl =
                pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

            return paymentUrl;
        }


        public PaymentInformationResponse PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);

            return response;
        }

        public void SavePaymentOnline(PaymentInformationResponse response, int? invoiceId = null)
        {
            if (response == null) return;
            var payment = new PaymentOnline
            {
                Amount = decimal.TryParse(response.OrderDescription?.Split(' ').LastOrDefault(), out var amt) ? amt : 0,
                PaymentMethod = "VnPay",
                CreatedAt = DateTime.Now,
                Status = GetPaymentStatus(response.VnPayResponseCode),
                Note = GetPaymentNote(response.VnPayResponseCode, response.OrderDescription),
                BankAccId = response.OrderId ?? string.Empty,
                BankName = response.PaymentId ?? string.Empty,
                InvoiceId = invoiceId
            };
            _paymentOnlineRepository.Add(payment);

            if (invoiceId.HasValue)
            {
                if (payment.Status == "Success")
                {
                    var invoice = _ticketInvoiceService.GetByIdAsync(invoiceId.Value).GetAwaiter().GetResult();
                    if (invoice != null)
                    {
                        invoice.Status = "Success";
                        _ticketInvoiceService.UpdateAsync(invoice).GetAwaiter().GetResult();

                        // Trừ điểm nếu có sử dụng điểm
                        if (((int?)invoice.ScoresUsed ?? 0) > 0)
                        {
                            _scoreService.UseScoreForInvoiceAsync(invoice.Userid, invoice.InvoiceId, (int?)invoice.ScoresUsed ?? 0).GetAwaiter().GetResult();
                        }
                        // Cộng điểm thưởng cho user
                        _scoreService.AddScoreForInvoiceAsync(invoice.Userid, invoice.InvoiceId, invoice.TotalPrice).GetAwaiter().GetResult();
                    }
                }
                else
                {
                    // Đã cập nhật trạng thái ghế trong khi xóa
                    _ticketInvoiceService.DeleteAsync(invoiceId.Value).GetAwaiter().GetResult();
                }
                
            }
        }

        private string GetPaymentStatus(string vnPayResponseCode)
        {
            return vnPayResponseCode switch
            {
                "00" => "Success",
                "24" => "Cancelled",
                "INVALID_SIGNATURE" => "Invalid",
                _ => "Failed"
            };
        }

        private string GetPaymentNote(string vnPayResponseCode, string orderDescription)
        {
            var baseNote = orderDescription ?? "";
            var statusNote = vnPayResponseCode switch
            {
                "00" => " - Thanh toán thành công",
                "24" => " - Khách hàng hủy giao dịch",
                "INVALID_SIGNATURE" => " - Chữ ký không hợp lệ",
                _ => $" - Giao dịch thất bại (Mã lỗi: {vnPayResponseCode})"
            };
            return baseNote + statusNote;
        }
    }
}
