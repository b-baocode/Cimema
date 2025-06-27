using System.ComponentModel.DataAnnotations;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class PaymentInformationRequest
    {
        // public string OrderType { get; set; }
        // [Required(ErrorMessage = "Amount is required")]
        // [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be a positive number greater than 0")]
        // public double Amount { get; set; }

        [Required(ErrorMessage = "Order description is required")]
        [StringLength(500, ErrorMessage = "Order description cannot exceed 500 characters")]
        public string OrderDescription { get; set; }

        // public string Name { get; set; }

        [Required(ErrorMessage = "InvoiceId is required")]
        public int InvoiceId { get; set; }
    }
}
