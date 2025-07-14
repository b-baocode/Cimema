using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.DTO.RequestModel.DashBoardRequest;
using MV.ApplicationLayer.DTO.ResponseModel.DashBoardResponse;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.InfrastructureLayer.DBContext;
using MV.DomainLayer.Entities;

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

        

        public async Task<MovieRevenueChartResponse> GetMovieRevenueChartAsync(string type, DateTime startDate, DateTime endDate, List<int> movieIds = null)
        {
            var startDateOnly = startDate.Date;
            var endDateOnly = endDate.Date.AddDays(1);
            
            Console.WriteLine($"Debug: Querying from {startDateOnly} to {endDateOnly}");
            
            // Bước 1: Lấy tất cả TicketInvoices trong khoảng thời gian
            var invoices = await _context.TicketInvoices
                .Where(ti => (ti.Status == "Success" || ti.Status == "Checked") &&
                             ti.CreatedAt >= startDateOnly && ti.CreatedAt < endDateOnly)
                .ToListAsync();
            
            Console.WriteLine($"Debug: Found {invoices.Count} invoices");
            
            // Bước 2: Lấy tất cả TicketDetails từ các invoices
            var ticketDetails = new List<TicketDetail>();
            foreach (var invoice in invoices)
            {
                var details = await _context.TicketDetails
                    .Include(td => td.ShowtimeInstance)
                        .ThenInclude(sri => sri.Showtime)
                            .ThenInclude(s => s.Movie)
                    .Where(td => td.InvoiceId == invoice.InvoiceId &&
                                 td.ShowtimeInstance.Showtime.MovieId != null &&
                                 td.ShowtimeInstance.Showtime.Movie != null)
                    .ToListAsync();
                ticketDetails.AddRange(details);
            }
            
            Console.WriteLine($"Debug: Found {ticketDetails.Count} ticket details");
            
            // Bước 3: Filter theo movieIds nếu có
            if (movieIds != null && movieIds.Any())
            {
                ticketDetails = ticketDetails.Where(td => movieIds.Contains(td.ShowtimeInstance.Showtime.MovieId.Value)).ToList();
                Console.WriteLine($"Debug: After movie filter: {ticketDetails.Count} ticket details");
            }
            
            // Bước 4: Group theo movie
            var movieGroups = ticketDetails
                .GroupBy(td => new { 
                    MovieId = td.ShowtimeInstance.Showtime.MovieId.Value, 
                    Title = td.ShowtimeInstance.Showtime.Movie.Title 
                })
                .ToList();
            
            Console.WriteLine($"Debug: Found {movieGroups.Count} movie groups");
            
            var data = new List<MovieRevenueChartItem>();
            foreach (var movieGroup in movieGroups)
            {
                Console.WriteLine($"Debug: Processing movie {movieGroup.Key.Title} with {movieGroup.Count()} tickets");
                
                List<MovieRevenueChartPoint> points;
                if (type == "day")
                {
                    points = movieGroup
                        .GroupBy(td => td.Invoice.CreatedAt.Date)
                        .Select(g => new MovieRevenueChartPoint
                        {
                            Label = g.Key.ToString("yyyy-MM-dd"),
                            Revenue = g.Sum(td => td.TicketPrice),
                            TotalOrders = g.Count()
                        }).OrderBy(p => p.Label).ToList();
                }
                else if (type == "month")
                {
                    points = movieGroup
                        .GroupBy(td => new { td.Invoice.CreatedAt.Year, td.Invoice.CreatedAt.Month })
                        .Select(g => new MovieRevenueChartPoint
                        {
                            Label = g.Key.Year + "-" + g.Key.Month.ToString("D2"),
                            Revenue = g.Sum(td => td.TicketPrice),
                            TotalOrders = g.Count()
                        }).OrderBy(p => p.Label).ToList();
                }
                else // year
                {
                    points = movieGroup
                        .GroupBy(td => td.Invoice.CreatedAt.Year)
                        .Select(g => new MovieRevenueChartPoint
                        {
                            Label = g.Key.ToString(),
                            Revenue = g.Sum(td => td.TicketPrice),
                            TotalOrders = g.Count()
                        }).OrderBy(p => p.Label).ToList();
                }
                
                Console.WriteLine($"Debug: Movie {movieGroup.Key.Title} has {points.Count} points");
                
                data.Add(new MovieRevenueChartItem
                {
                    MovieId = movieGroup.Key.MovieId,
                    MovieName = movieGroup.Key.Title,
                    Points = points
                });
            }
            
            return new MovieRevenueChartResponse
            {
                Type = type,
                StartDate = startDate,
                EndDate = endDate,
                Data = data
            };
        }

        public async Task<FoodRevenueChartResponse> GetFoodRevenueChartAsync(string type, DateTime startDate, DateTime endDate, List<int> foodIds = null)
        {
            var startDateOnly = startDate.Date;
            var endDateOnly = endDate.Date.AddDays(1);
            
            Console.WriteLine($"Debug Food: Querying from {startDateOnly} to {endDateOnly}");
            
            // Bước 1: Lấy tất cả TicketInvoices trong khoảng thời gian
            var invoices = await _context.TicketInvoices
                .Where(ti => (ti.Status == "Success" || ti.Status == "Checked") &&
                             ti.CreatedAt >= startDateOnly && ti.CreatedAt < endDateOnly)
                .ToListAsync();
            
            Console.WriteLine($"Debug Food: Found {invoices.Count} invoices");
            
            // Bước 2: Lấy tất cả TicketInvoiceFoodItems từ các invoices
            var foodItems = new List<TicketInvoiceFoodItem>();
            foreach (var invoice in invoices)
            {
                var items = await _context.TicketInvoiceFoodItems
                    .Include(tifi => tifi.Food)
                    .Where(tifi => tifi.InvoiceId == invoice.InvoiceId &&
                                   tifi.Food != null)
                    .ToListAsync();
                foodItems.AddRange(items);
            }
            
            Console.WriteLine($"Debug Food: Found {foodItems.Count} food items");
            
            // Bước 3: Filter theo foodIds nếu có
            if (foodIds != null && foodIds.Any())
            {
                foodItems = foodItems.Where(tifi => foodIds.Contains(tifi.FoodId)).ToList();
                Console.WriteLine($"Debug Food: After food filter: {foodItems.Count} food items");
            }
            
            // Bước 4: Group theo food
            var foodGroups = foodItems
                .GroupBy(tifi => new { 
                    FoodId = tifi.FoodId, 
                    FoodName = tifi.Food.FoodName 
                })
                .ToList();
            
            Console.WriteLine($"Debug Food: Found {foodGroups.Count} food groups");
            
            var data = new List<FoodRevenueChartItem>();
            foreach (var foodGroup in foodGroups)
            {
                Console.WriteLine($"Debug Food: Processing food {foodGroup.Key.FoodName} with {foodGroup.Count()} items");
                
                List<FoodRevenueChartPoint> points;
                if (type == "day")
                {
                    points = foodGroup
                        .GroupBy(tifi => tifi.Invoice.CreatedAt.Date)
                        .Select(g => new FoodRevenueChartPoint
                        {
                            Label = g.Key.ToString("yyyy-MM-dd"),
                            Revenue = g.Sum(tifi => tifi.TotalFoodPrice),
                            TotalOrders = g.Count()
                        }).OrderBy(p => p.Label).ToList();
                }
                else if (type == "month")
                {
                    points = foodGroup
                        .GroupBy(tifi => new { tifi.Invoice.CreatedAt.Year, tifi.Invoice.CreatedAt.Month })
                        .Select(g => new FoodRevenueChartPoint
                        {
                            Label = g.Key.Year + "-" + g.Key.Month.ToString("D2"),
                            Revenue = g.Sum(tifi => tifi.TotalFoodPrice),
                            TotalOrders = g.Count()
                        }).OrderBy(p => p.Label).ToList();
                }
                else // year
                {
                    points = foodGroup
                        .GroupBy(tifi => tifi.Invoice.CreatedAt.Year)
                        .Select(g => new FoodRevenueChartPoint
                        {
                            Label = g.Key.ToString(),
                            Revenue = g.Sum(tifi => tifi.TotalFoodPrice),
                            TotalOrders = g.Count()
                        }).OrderBy(p => p.Label).ToList();
                }
                
                Console.WriteLine($"Debug Food: Food {foodGroup.Key.FoodName} has {points.Count} points");
                
                data.Add(new FoodRevenueChartItem
                {
                    FoodId = foodGroup.Key.FoodId,
                    FoodName = foodGroup.Key.FoodName,
                    Points = points
                });
            }
            
            return new FoodRevenueChartResponse
            {
                Type = type,
                StartDate = startDate,
                EndDate = endDate,
                Data = data
            };
        }
    }
} 