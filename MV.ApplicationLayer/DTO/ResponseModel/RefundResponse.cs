namespace MV.ApplicationLayer.DTO.ResponseModel
{
    public class RefundResponse
    {
        public int InvoiceId { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public string RefundReason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? IneligibilityReason { get; set; }
        public string? PaymentMethod { get; set; }
        public string? UserEmail { get; set; }
        public string? UserName { get; set; }
    }
}