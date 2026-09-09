using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationRequest model, double amount, HttpContext context);
        PaymentInformationResponse PaymentExecute(IQueryCollection collections);
        Task SavePaymentOnline(PaymentInformationResponse response, int? invoiceId = null);
    }
}
