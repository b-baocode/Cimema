using System.Threading.Tasks;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;
using Microsoft.EntityFrameworkCore;

namespace MV.InfrastructureLayer.Repositories
{
    public class TicketInvoiceRepository : ITicketInvoiceRepository
    {
        private readonly MovietheatermanagementContext _context;
        public TicketInvoiceRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TicketInvoice invoice)
        {
            _context.TicketInvoices.Add(invoice);
            await _context.SaveChangesAsync();
        }

        public async Task<TicketInvoice?> GetByIdAsync(int invoiceId)
        {
            return await _context.TicketInvoices
                .Include(ti => ti.TicketDetails)
                .Include(ti => ti.TicketInvoiceFoodItems)
                .Include(ti => ti.Promotion)
                .FirstOrDefaultAsync(ti => ti.InvoiceId == invoiceId);
        }

        public async Task<List<TicketInvoice>> GetByUserIdAsync(string userId)
        {
            return await _context.TicketInvoices
                .Include(ti => ti.TicketDetails)
                .Include(ti => ti.TicketInvoiceFoodItems)
                .Include(ti => ti.Promotion)
                .Where(ti => ti.Userid == userId)
                .ToListAsync();
        }

        public async Task<List<TicketInvoice>> GetAllAsync()
        {
            return await _context.TicketInvoices
                .Include(ti => ti.TicketDetails)
                .Include(ti => ti.TicketInvoiceFoodItems)
                .Include(ti => ti.Promotion)
                .ToListAsync();
        }

        public async Task UpdateAsync(TicketInvoice invoice)
        {
            _context.TicketInvoices.Update(invoice);
            await _context.SaveChangesAsync();
        }
    }
} 