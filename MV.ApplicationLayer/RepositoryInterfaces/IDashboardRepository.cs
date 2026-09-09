using MV.ApplicationLayer.DTO.RequestModel.DashBoardRequest;
using MV.ApplicationLayer.DTO.ResponseModel.DashBoardResponse;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IDashboardRepository
    {
        Task<RevenueResponse> GetRevenueAsync(DateTime startDate, DateTime endDate);
        Task<RevenueChartResponse> GetRevenueChartAsync(RevenueChartRequest request);
        Task<MovieRevenueChartResponse> GetMovieRevenueChartAsync(string type, DateTime startDate, DateTime endDate, List<int> movieIds = null);
        Task<FoodRevenueChartResponse> GetFoodRevenueChartAsync(string type, DateTime startDate, DateTime endDate, List<int> foodIds = null);
    }
}