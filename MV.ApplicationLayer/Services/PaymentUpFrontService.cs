using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel.MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;

namespace MV.ApplicationLayer.Services
{
        public class PaymentUpFrontService : IPaymentUpFrontService
        {
            private readonly IUnitOfWork _unitOfWork;

            public PaymentUpFrontService(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork;
            }

            public async Task<PaymentUpFrontResponse> CreatePaymentUpFrontAsync(PaymentUpFrontRequest paymentRequest)
            {
                // Lấy thông tin TicketInvoice để có TotalAmount và ScoreDiscountAmount
                var invoice = await _unitOfWork.ticketInvoiceRepository.GetByIdAsync(paymentRequest.InvoiceId);
                if (invoice == null)
                {
                    throw new Exception("Invoice not found.");
                }

                // Tính toán số tiền thực tế cần thanh toán (sau khi trừ điểm tích lũy)
            var actualAmountToPay = invoice.TotalPrice - (invoice.ScoreDiscountAmount ?? 0m);

                // Tính toán số tiền thừa
                var remainChange = paymentRequest.CustomerGive - actualAmountToPay;

                if (remainChange < 0)
                {
                    throw new Exception("Số tiền khách đưa không đủ để thanh toán.");
                }

                var payment = new PaymentUpFront
                {
                    TotalAmount = actualAmountToPay, // Số tiền thực tế cần thanh toán
                    CustomerGive = paymentRequest.CustomerGive,
                    RemainChange = remainChange,
                    InvoiceId = paymentRequest.InvoiceId,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    Status = "Completed"
                };

                await _unitOfWork.paymentUpFrontRepository.AddPaymentUpFrontAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                return new PaymentUpFrontResponse
                {
                    PaymentUpFrontId = payment.PaymentUpFrontId,
                    TotalAmount = payment.TotalAmount,
                    CustomerGive = payment.CustomerGive,
                    RemainChange = payment.RemainChange,
                ScoreDiscountAmount = invoice.ScoreDiscountAmount ?? 0m,
                    CreatedAt = payment.CreatedAt,
                    Status = payment.Status,
                    InvoiceId = payment.InvoiceId
                };
            }

            public async Task<bool> DeletePaymentUpFrontAsync(int id)
            {
                var result = await _unitOfWork.paymentUpFrontRepository.DeletePaymentUpFrontAsync(id);
                if (result == null)
                {
                    return false;
                }
                await _unitOfWork.SaveChangesAsync();
                return true;
            }

            public async Task<IEnumerable<PaymentUpFrontResponse>> GetAllPaymentUpFrontsAsync()
            {
                var payments = await _unitOfWork.paymentUpFrontRepository.GetAllPaymentUpFrontsAsync();
                var responses = new List<PaymentUpFrontResponse>();

                foreach (var p in payments)
                {
                    var invoice = p.InvoiceId.HasValue
                        ? await _unitOfWork.ticketInvoiceRepository.GetByIdAsync(p.InvoiceId.Value)
                        : null;

                    responses.Add(new PaymentUpFrontResponse
                    {
                        PaymentUpFrontId = p.PaymentUpFrontId,
                        TotalAmount = p.TotalAmount,
                        CustomerGive = p.CustomerGive,
                        RemainChange = p.RemainChange,
                        ScoreDiscountAmount = invoice?.ScoreDiscountAmount ?? 0m,
                        CreatedAt = p.CreatedAt,
                        Status = p.Status,
                        InvoiceId = p.InvoiceId
                    });
                }

                return responses;
            }

            public async Task<PaymentUpFrontResponse> GetPaymentUpFrontByIdAsync(int id)
            {
                var p = await _unitOfWork.paymentUpFrontRepository.GetPaymentUpFrontByIdAsync(id);
                if (p == null)
                {
                    return null;
                }

                var invoice = p.InvoiceId.HasValue
                    ? await _unitOfWork.ticketInvoiceRepository.GetByIdAsync(p.InvoiceId.Value)
                    : null;

                return new PaymentUpFrontResponse
                {
                    PaymentUpFrontId = p.PaymentUpFrontId,
                    TotalAmount = p.TotalAmount,
                    CustomerGive = p.CustomerGive,
                    RemainChange = p.RemainChange,
                    ScoreDiscountAmount = invoice?.ScoreDiscountAmount ?? 0m,
                    CreatedAt = p.CreatedAt,
                    Status = p.Status,
                    InvoiceId = p.InvoiceId
                };
            }

            public async Task<PaymentUpFrontResponse> UpdatePaymentUpFrontAsync(int id, PaymentUpFrontRequest paymentRequest)
            {
                var payment = await _unitOfWork.paymentUpFrontRepository.GetPaymentUpFrontByIdAsync(id);
                if (payment == null)
                {
                    return null;
                }

                // Lấy thông tin TicketInvoice để có TotalAmount và ScoreDiscountAmount
                var invoice = await _unitOfWork.ticketInvoiceRepository.GetByIdAsync(paymentRequest.InvoiceId);
                if (invoice == null)
                {
                    throw new Exception("Invoice not found.");
                }

                // Tính toán số tiền thực tế cần thanh toán (sau khi trừ điểm tích lũy)
            var actualAmountToPay = invoice.TotalPrice - (invoice.ScoreDiscountAmount ?? 0m);

                // Tính toán số tiền thừa
                var remainChange = paymentRequest.CustomerGive - actualAmountToPay;

                if (remainChange < 0)
                {
                    throw new Exception("Số tiền khách đưa không đủ để thanh toán.");
                }

                payment.TotalAmount = actualAmountToPay;
                payment.CustomerGive = paymentRequest.CustomerGive;
                payment.RemainChange = remainChange;
                payment.InvoiceId = paymentRequest.InvoiceId;

                await _unitOfWork.paymentUpFrontRepository.UpdatePaymentUpFrontAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                return new PaymentUpFrontResponse
                {
                    PaymentUpFrontId = payment.PaymentUpFrontId,
                    TotalAmount = payment.TotalAmount,
                    CustomerGive = payment.CustomerGive,
                    RemainChange = payment.RemainChange,
                ScoreDiscountAmount = invoice.ScoreDiscountAmount ?? 0m,
                CreatedAt = payment.CreatedAt,
                Status = payment.Status,
                InvoiceId = payment.InvoiceId
            };
        }

        public async Task<PaymentUpFrontResponse> UndoPaymentUpFrontAsync(int id)
        {
            var payment = await _unitOfWork.paymentUpFrontRepository.UndoPaymentUpFrontAsync(id);
            if (payment == null)
            {
                return null;
            }
            await _unitOfWork.SaveChangesAsync();
            return new PaymentUpFrontResponse
            {
                PaymentUpFrontId = payment.PaymentUpFrontId,
                TotalAmount = payment.TotalAmount,
                CustomerGive = payment.CustomerGive,
                RemainChange = payment.RemainChange,
                ScoreDiscountAmount = 0m, // Nếu cần lấy từ invoice thì có thể truy vấn thêm
                    CreatedAt = payment.CreatedAt,
                    Status = payment.Status,
                    InvoiceId = payment.InvoiceId
                };
        }
    }
}
