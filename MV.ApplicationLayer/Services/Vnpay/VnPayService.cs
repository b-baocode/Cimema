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

        public VnpayService(IConfiguration configuration, IPaymentOnlineRepository paymentOnlineRepository)
        {
            _configuration = configuration;
            _paymentOnlineRepository = paymentOnlineRepository;
        }

        public string CreatePaymentUrl(PaymentInformationRequest model, HttpContext context)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary();
            var urlCallBack = _configuration["PaymentCallBack:ReturnUrl"];

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((int)model.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"{model.Name} {model.OrderDescription} {model.Amount}");
            pay.AddRequestData("vnp_OrderType", model.OrderType);
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
            if (response == null || !response.Success) return;
            var payment = new PaymentOnline
            {
                Amount = decimal.TryParse(response.OrderDescription?.Split(' ').LastOrDefault(), out var amt) ? amt : 0,
                PaymentMethod = "VnPay",
                CreatedAt = DateTime.Now,
                Status = response.VnPayResponseCode == "00" ? "Success" : "Failed",
                Note = response.OrderDescription,
                BankAccId = response.OrderId ?? string.Empty,
                BankName = response.PaymentId ?? string.Empty,
                InvoiceId = invoiceId
            };
            _paymentOnlineRepository.Add(payment);
        }
    }
}
