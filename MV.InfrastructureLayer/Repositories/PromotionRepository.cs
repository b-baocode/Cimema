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
            var query = _context.Promotions.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(p =>
                    p.PromotionName.ToLower().Contains(keyword) ||
                    p.Description.ToLower().Contains(keyword));
            }

            return await query
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetTotalPromotionsAsync(string? keyword)
        {
            var query = _context.Promotions.AsQueryable();

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
            return await _context.Promotions.FirstOrDefaultAsync(p => p.PromotionId == id);
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
                _context.Promotions.Remove(promotion);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Promotion?> GetLastPromotionAsync()
        {
            return await _context.Promotions
                .OrderByDescending(p => p.PromotionId)
                .FirstOrDefaultAsync();
        }
    }
} 