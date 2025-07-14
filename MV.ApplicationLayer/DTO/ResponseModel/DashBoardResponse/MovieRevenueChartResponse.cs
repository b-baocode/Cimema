using System;
using System.Collections.Generic;

namespace MV.ApplicationLayer.DTO.ResponseModel.DashBoardResponse
{
    public class MovieRevenueChartPoint
    {
        public string Label { get; set; }
        public decimal Revenue { get; set; }
        public int TotalOrders { get; set; }
    }

    public class MovieRevenueChartItem
    {
        public int MovieId { get; set; }
        public string MovieName { get; set; }
        public List<MovieRevenueChartPoint> Points { get; set; }
    }

    public class MovieRevenueChartResponse
    {
        public string Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<MovieRevenueChartItem> Data { get; set; }
    }
} 