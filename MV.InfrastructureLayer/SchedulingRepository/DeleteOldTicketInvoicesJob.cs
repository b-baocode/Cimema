using Microsoft.Extensions.Logging;
using MV.ApplicationLayer.RepositoryInterfaces;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MV.InfrastructureLayer.SchedulingRepository
{
    public class DeleteOldTicketInvoicesJob : IJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteOldTicketInvoicesJob> _logger;

        public DeleteOldTicketInvoicesJob(IUnitOfWork unitOfWork, ILogger<DeleteOldTicketInvoicesJob> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                            // Muốn đổi thành mốc thời gian thì:
            // Muốn đổi thành ngày AddDays(-1) tức là 1 ngày
            // Muốn đổi thành tháng AddMonths(-1) tức là 1 tháng
            // Muốn đổi thành năm AddYears(-1) tức là 1 năm
            // Muốn đổi thành giờ AddHours(-1) tức là 1 giờ
            // Muốn đổi thành phút AddMinutes(-1) tức là 1 phút
            // Muốn đổi thành giây AddSeconds(-1) tức là 1 giây
            // Muốn đổi thành mili giây AddMilliseconds(-1) tức là 1 mili giây
            // Muốn đổi thành micro giây AddMicroseconds(-1) tức là 1 micro giây
            // var oneHourAgo = DateTime.UtcNow.AddHours(-1); // Sửa nếu muốn thay đổi mốc thời gian (UtcNow là giờ Quốc Tế)
                var oneMonthAgo = DateTime.Now.AddMonths(-1); // Đổi thành 1 phút để test nhanh
                var oldInvoices = _unitOfWork
                    .ticketInvoiceRepository
                    .GetAll()
                    .Where(i => i.CreatedAt < oneMonthAgo)
                    .ToList();

                if (oldInvoices.Any())
                {
                    foreach (var invoice in oldInvoices)
                    {
                        await _unitOfWork.ticketInvoiceRepository.DeleteAsync(invoice.InvoiceId);
                    }
                    var affected = await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation($"Deleted {oldInvoices.Count} old ticket invoices. SaveChanges affected: {affected}");
                }
                else
                {
                    _logger.LogInformation("No old ticket invoices to delete.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting old ticket invoices");
            }
        }
    }
} 