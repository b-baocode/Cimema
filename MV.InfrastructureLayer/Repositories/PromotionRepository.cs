using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MV.InfrastructureLayer.Repositories
{
    public class PromotionRepository : IPromotionRepository
    {
        private readonly MovietheatermanagementContext _context;

        public PromotionRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Promotion>> GetPromotionsAsync(string? keyword, int skip, int take)
        {
            var query = _context.Promotions
                // Lọc Status
                // .Where(p => p.Status != "InActive")
                // .AsQueryable();
                .AsQueryable(); // Không lọc theo status nữa, hiển thị tất cả promotion

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(p =>
                    p.PromotionName.ToLower().Contains(keyword) ||
                    p.Description.ToLower().Contains(keyword));
            }

            var promotions = await query
                .OrderByDescending(p => p.StartDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            // Update status based on current date
            // var now = DateTime.Now;
            // foreach (var promotion in promotions)
            // {
            //     var newStatus = DeterminePromotionStatus(promotion.StartDate, promotion.EndDate);
            //     if (promotion.Status != newStatus)
            //     {
            //         promotion.Status = newStatus;
            //         _context.Promotions.Update(promotion);
            //     }
            // }
            // await _context.SaveChangesAsync(); // SaveChanges: Set Status in Database

            return promotions;

            /*
             * // Update status in memory only
                var now = DateTime.Now;
                foreach (var promotion in promotions)
                {
                    promotion.Status = DeterminePromotionStatus(promotion.StartDate, promotion.EndDate);
                }
                return promotions;
             */
        }

        public async Task<IEnumerable<Promotion>> GetComingSoonPromotionsAsync(int skip, int take)
        {
            var now = DateTime.Now;
            var promotions = await _context.Promotions
                .Where(p => p.Status != "InActive" && p.StartDate > now)
                .OrderBy(p => p.StartDate) // Sắp xếp theo ngày bắt đầu tăng dần
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            // Cập nhật status cho tất cả promotion thành ComingSoon
            foreach (var promotion in promotions)
            {
                promotion.Status = "ComingSoon";
            }

            return promotions;
        }

        public async Task<int> GetTotalComingSoonPromotionsAsync()
        {
            var now = DateTime.Now;
            return await _context.Promotions
                .Where(p => p.Status != "InActive" && p.StartDate > now)
                .CountAsync();
        }

        public async Task<int> GetTotalPromotionsAsync(string? keyword)
        {
            var query = _context.Promotions
                // Lọc Status, lấy tất cả Status nhưng không lấy Status InActive
                // .Where(p => p.Status != "InActive")
                // .AsQueryable();
                .AsQueryable(); // Không lọc theo status nữa, tính tất cả promotion

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(p =>
                    p.PromotionName.ToLower().Contains(keyword) ||
                    p.Description.ToLower().Contains(keyword));
            }

            return await query.CountAsync();
        }

        public async Task<Promotion?> GetPromotionByIdAsync(int id)
        {
            var promotion = await _context.Promotions.FirstOrDefaultAsync(p => p.PromotionId == id);
            
            //if (promotion != null && promotion.Status != "InActive")
            //{
                // Update status based on current date
            // if (promotion != null && promotion.Status != "InActive")
            //if (promotion != null)
            //{
                // Update status based on current date (cập nhật cho tất cả promotion)
                // var newStatus = DeterminePromotionStatus(promotion.StartDate, promotion.EndDate);
                // if (promotion.Status != newStatus)
                // {
                //     promotion.Status = newStatus;
                //     _context.Promotions.Update(promotion);
                //     await _context.SaveChangesAsync();
                // }
            //}
            //}
            return promotion;
        }

        public async Task<bool> IsPromotionNameExistsAsync(string promotionName)
        {
            return await _context.Promotions.AnyAsync(p => p.PromotionName == promotionName);
        }

        public async Task<Promotion> CreatePromotionAsync(Promotion promotion)
        {
            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();
            return promotion;
        }

        public async Task<Promotion> UpdatePromotionAsync(Promotion promotion)
        {
            _context.Promotions.Update(promotion);
            await _context.SaveChangesAsync();
            return promotion;
        }

        public async Task DeletePromotionAsync(int id)
        {
            var promotion = await _context.Promotions
                .Include(p => p.TicketInvoices)
                .FirstOrDefaultAsync(p => p.PromotionId == id);

            if (promotion != null)
            {
                if (promotion.TicketInvoices.Any())
                {
                    throw new InvalidOperationException("Cannot delete promotion because it is associated with ticket invoices.");
                }
                promotion.Status = "InActive";
                _context.Promotions.Update(promotion);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Promotion?> GetLastPromotionAsync()
        {
            return await _context.Promotions
                .OrderByDescending(p => p.PromotionId)
                .FirstOrDefaultAsync();
        }

        private string DeterminePromotionStatus(DateTime startDate, DateTime endDate)
        {
            var now = DateTime.Now;
            
            if (startDate > now)
            {
                return "ComingSoon";
            }
            else if (startDate <= now && endDate >= now)
            {
                return "Active";
            }
            else if (endDate < now)
            {
                return "Expired";
            }
            else
            {
                return "InActive";
            }
        }
    }
}