using System.ComponentModel.DataAnnotations;

namespace MV.ApplicationLayer.DTO.RequestModel
{
    public class RefundRequest
    {
        [Required]
        public int InvoiceId { get; set; }
        
        [Required]
        [StringLength(500)]
        public string RefundReason { get; set; } = string.Empty;
        
        [Required]
        public string UserId { get; set; } = string.Empty; // Để validate quyền
        
        public string? AdminId { get; set; } // ID của admin/manager thực hiện refund
    }
}