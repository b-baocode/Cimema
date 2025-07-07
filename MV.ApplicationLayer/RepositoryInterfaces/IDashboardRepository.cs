using System;
using System.Threading.Tasks;
using MV.ApplicationLayer.DTO.ResponseModel.DashBoardResponse;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IDashboardRepository
    {
        Task<RevenueResponse> GetRevenueAsync(DateTime startDate, DateTime endDate);
    }
} 