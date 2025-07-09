using Microsoft.Extensions.Logging;
using MV.InfrastructureLayer;
using MV.ApplicationLayer.RepositoryInterfaces;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MV.InfrastructureLayer.SchedulingRepository
{
    public class DeleteOldSuccessfulPaymentsJob : IJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteOldSuccessfulPaymentsJob> _logger;

        public DeleteOldSuccessfulPaymentsJob(IUnitOfWork unitOfWork, ILogger<DeleteOldSuccessfulPaymentsJob> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
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
            var oneMonthAgo = DateTime.Now.AddMonths(-1); // (Now: Lấy theo giờ Việt Nam)
            var oldPayments = _unitOfWork
                .paymentOnlineRepository
                .GetAll()
                .Where(p => p.Status == "Success" && p.CreatedAt < oneMonthAgo) // Sửa nếu muốn thay đổi mốc thời gian
                .ToList();

            if (oldPayments.Any())
            {
                foreach (var payment in oldPayments)
                {
                    _unitOfWork.paymentOnlineRepository.Delete(payment);
                }

                await _unitOfWork.SaveChangesAsync(); // Save

                _logger.LogInformation($"Deleted {oldPayments.Count} old successful payments.");
            }
            else
            {
                _logger.LogInformation("No old successful payments to delete.");
            }
        }
    }
} 