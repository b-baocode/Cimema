using MV.ApplicationLayer.DTO.ResponseModel.DashBoardResponse;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;

namespace MV.ApplicationLayer.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDashboardRepository _dashboardRepository;

       public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<RevenueResponse> GetRevenueAsync(string period, DateTime? startDate, DateTime? endDate, int? year = null, int? month = null, int? day = null)
        {
            DateTime queryStart, queryEnd;

            // Xử lý logic thời gian dựa trên period và các tham số year, month, day
            switch (period?.ToLower())
            {
                case "today":
                    if (year.HasValue && month.HasValue && day.HasValue)
                    {
                        // FE truyền ngày cụ thể
                        queryStart = new DateTime(year.Value, month.Value, day.Value);
                        queryEnd = queryStart.AddDays(1).AddSeconds(-1);
                    }
                    else
                    {
                        // Hôm nay (mặc định)
                        queryStart = DateTime.Today;
                        queryEnd = DateTime.Today.AddDays(1).AddSeconds(-1);
                    }
                    break;

                case "month":
                    if (year.HasValue && month.HasValue)
                    {
                        // FE truyền tháng cụ thể
                        queryStart = new DateTime(year.Value, month.Value, 1);
                        queryEnd = queryStart.AddMonths(1).AddSeconds(-1);
                    }
                    else
                    {
                        // Tháng hiện tại (mặc định)
                        queryStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        queryEnd = queryStart.AddMonths(1).AddSeconds(-1);
                    }
                    break;

                case "year":
                    if (year.HasValue)
                    {
                        // FE truyền năm cụ thể
                        queryStart = new DateTime(year.Value, 1, 1);
                        queryEnd = new DateTime(year.Value, 12, 31, 23, 59, 59);
                    }
                    else
                    {
                        // Năm hiện tại (mặc định)
                        queryStart = new DateTime(DateTime.Now.Year, 1, 1);
                        queryEnd = new DateTime(DateTime.Now.Year, 12, 31, 23, 59, 59);
                    }
                    break;

                case "custom":
                    // Sử dụng startDate và endDate nếu có
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        queryStart = startDate.Value.Date;
                        queryEnd = endDate.Value.Date.AddDays(1).AddSeconds(-1);
                    }
                    else
                    {
                        // Default to today
                        queryStart = DateTime.Today;
                        queryEnd = DateTime.Today.AddDays(1).AddSeconds(-1);
                    }
                    break;

                default:
                    // Default to today
                    queryStart = DateTime.Today;
                    queryEnd = DateTime.Today.AddDays(1).AddSeconds(-1);
                    break;
            }

            return await _unitOfWork.dashboardRepository.GetRevenueAsync(queryStart, queryEnd);
        }
    }
} 