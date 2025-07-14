using System;
using System.Collections.Generic;

namespace MV.ApplicationLayer.DTO.ResponseModel.DashBoardResponse
{
    public class FoodRevenueChartPoint
    {
        public string Label { get; set; }
        public decimal Revenue { get; set; }
        public int TotalOrders { get; set; }
    }

    public class FoodRevenueChartItem
    {
        public int FoodId { get; set; }
        public string FoodName { get; set; }
        public List<FoodRevenueChartPoint> Data { get; set; }
    }

    public class FoodRevenueChartResponse
    {
        public string Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<FoodRevenueChartItem> Data { get; set; }
    }
} 