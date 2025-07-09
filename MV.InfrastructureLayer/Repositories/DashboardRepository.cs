using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.DTO.RequestModel.DashBoardRequest;
using MV.ApplicationLayer.DTO.ResponseModel.DashBoardResponse;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.InfrastructureLayer.DBContext;

namespace MV.InfrastructureLayer.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly MovietheatermanagementContext _context;

        public DashboardRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }

        public async Task<RevenueResponse> GetRevenueAsync(DateTime startDate, DateTime endDate)
        {
            var startDateOnly = startDate.Date;
            var endDateOnly = endDate.Date.AddDays(1);
            var revenueData = await _context.TicketInvoices
                .Where(ti => (ti.Status == "Success" || ti.Status == "Checked") &&
                             ti.CreatedAt >= startDateOnly && ti.CreatedAt < endDateOnly)
                .GroupBy(ti => 1)
                .Select(g => new
                {
                    TotalRevenue = g.Sum(ti => ti.ScoreDiscountAmount),
                    TotalOrders = g.Count()
                })
                .FirstOrDefaultAsync();

            return new RevenueResponse
            {
                Revenue = revenueData?.TotalRevenue ?? 0,
                TotalOrders = revenueData?.TotalOrders ?? 0,
                StartDate = startDate,
                EndDate = endDate,
                Period = GetPeriodString(startDate, endDate)
            };
        }

        private string GetPeriodString(DateTime startDate, DateTime endDate)
        {
            if (startDate.Date == endDate.Date)
                return "today";
            else if (startDate.Month == endDate.Month && startDate.Year == endDate.Year)
                return "month";
            else if (startDate.Year == endDate.Year)
                return "year";
            else
                return "custom";
        }

        public async Task<RevenueChartResponse> GetRevenueChartAsync(RevenueChartRequest request)
        {
            var startDateOnly = request.StartDate.Date;
            var endDateOnly = request.EndDate.Date.AddDays(1);
            var query = _context.TicketInvoices
                .Where(ti => (ti.Status == "Success" || ti.Status == "Checked")
                    && ti.CreatedAt >= startDateOnly && ti.CreatedAt < endDateOnly);

            List<RevenueChartItem> data;

            if (request.Type == "day")
            {
                data = await query
                    .GroupBy(ti => ti.CreatedAt.Date)
                    .Select(g => new RevenueChartItem
                    {
                        Label = g.Key.ToString("yyyy-MM-dd"),
                        Revenue = g.Sum(ti => (decimal)ti.ScoreDiscountAmount),
                        TotalOrders = g.Count()
                    }).ToListAsync();
            }
            else if (request.Type == "month")
            {
                data = await query
                    .GroupBy(ti => new { ti.CreatedAt.Year, ti.CreatedAt.Month })
                    .Select(g => new RevenueChartItem
                    {
                        Label = g.Key.Year + "-" + g.Key.Month.ToString("D2"),
                        Revenue = g.Sum(ti => (decimal)ti.ScoreDiscountAmount),
                        TotalOrders = g.Count()
                    }).ToListAsync();
            }
            else // year
            {
                data = await query
                    .GroupBy(ti => ti.CreatedAt.Year)
                    .Select(g => new RevenueChartItem
                    {
                        Label = g.Key.ToString(),
                        Revenue = g.Sum(ti => (decimal)ti.ScoreDiscountAmount),
                        TotalOrders = g.Count()
                    }).ToListAsync();
            }

            return new RevenueChartResponse
            {
                Type = request.Type,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Data = data
            };
        }
    }
} 