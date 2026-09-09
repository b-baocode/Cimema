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
        [Required(ErrorMessage = "The amount of money the customer gives is mandatory.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal CustomerGive { get; set; }

        [Required(ErrorMessage = "Invoice's ID is required.")]
        public int InvoiceId { get; set; }
    }
}
