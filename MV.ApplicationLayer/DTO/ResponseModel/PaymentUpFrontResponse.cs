namespace MV.ApplicationLayer.DTO.ResponseModel
{
    using System;

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
