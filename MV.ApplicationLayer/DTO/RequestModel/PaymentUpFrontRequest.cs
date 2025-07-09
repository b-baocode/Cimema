using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class PaymentUpFrontRequest
    {
        [Required(ErrorMessage = "Số tiền khách đưa là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
        public decimal CustomerGive { get; set; }

        [Required(ErrorMessage = "InvoiceId là bắt buộc")]
        public int InvoiceId { get; set; }
    }
}
