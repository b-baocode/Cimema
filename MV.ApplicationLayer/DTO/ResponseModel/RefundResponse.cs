namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class RefundResponse
    {
        public int InvoiceId { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal RefundPercentage { get; set; }
        public string RefundReason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? ProcessedBy { get; set; }
        public string? Notes { get; set; }
        public bool IsEligibleForRefund { get; set; }
        public string? IneligibilityReason { get; set; }
        public string? PaymentMethod { get; set; }
        public string? UserEmail { get; set; }
        public string? UserName { get; set; }
    }
}