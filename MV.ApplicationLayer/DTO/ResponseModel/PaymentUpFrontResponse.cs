using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.DTO.ResponseModel
{
   using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class PaymentUpFrontResponse
    {
        public int PaymentUpFrontId { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal CustomerGive { get; set; }

        public decimal RemainChange { get; set; }

        public decimal ScoreDiscountAmount { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? Status { get; set; }

        public int? InvoiceId { get; set; }
    }
} 
}
