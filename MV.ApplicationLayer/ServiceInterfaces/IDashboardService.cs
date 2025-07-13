using System;
using System.Threading.Tasks;
using MV.ApplicationLayer.DTO.RequestModel.DashBoardRequest;
using MV.ApplicationLayer.DTO.ResponseModel.DashBoardResponse;
using System.Collections.Generic;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IDashboardService
    {
        Task<RevenueResponse> GetRevenueAsync(string period, DateTime? startDate, DateTime? endDate, int? year = null, int? month = null, int? day = null);
        Task<RevenueChartResponse> GetRevenueChartAsync(RevenueChartRequest request);
        Task<MovieRevenueChartResponse> GetMovieRevenueChartAsync(string type, DateTime startDate, DateTime endDate, List<int> movieIds = null);
        Task<FoodRevenueChartResponse> GetFoodRevenueChartAsync(string type, DateTime startDate, DateTime endDate, List<int> foodIds = null);
    }
} 