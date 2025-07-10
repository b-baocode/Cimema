using System;
using System.Collections.Generic;

namespace MV.ApplicationLayer.DTO.ResponseModel.DashBoardResponse
{
    public class RevenueChartResponse
    {
        public string Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<RevenueChartItem> Data { get; set; }
    }

    public class RevenueChartItem
    {
        public string Label { get; set; }
        public decimal Revenue { get; set; }
        public int TotalOrders { get; set; }
    }
} 