using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            var revenueData = await _context.TicketInvoices
                .Where(ti => (ti.Status == "Success" || ti.Status == "Checked") && 
                             ti.CreatedAt >= startDate && 
                             ti.CreatedAt <= endDate)
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
    }
} 