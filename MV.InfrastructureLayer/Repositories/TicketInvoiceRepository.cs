using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;

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
        }

        public async Task<TicketInvoice?> GetByIdAsync(int invoiceId)
        {
            return await _context.TicketInvoices
                .Include(ti => ti.TicketDetails)
                .Include(ti => ti.TicketInvoiceFoodItems)
                .Include(ti => ti.Promotion)
                .Include(ti => ti.User)
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

        public async Task<List<TicketInvoice>> GetByUserIdAndStatusAsync(string userId, string status)
        {
            return await _context.TicketInvoices
                .Include(ti => ti.TicketDetails)
                .Include(ti => ti.TicketInvoiceFoodItems)
                .Include(ti => ti.Promotion)
                .Where(ti => ti.Userid == userId && ti.Status == status)
                .ToListAsync();
        }

        public async Task<List<TicketInvoice>> GetAllAsync()
        {
            return await _context.TicketInvoices
                .Include(ti => ti.TicketDetails)
                .Include(ti => ti.TicketInvoiceFoodItems)
                .Include(ti => ti.Promotion)
                .OrderByDescending(ti => ti.CreatedAt)
                .Take(20)
                .ToListAsync();
        }

        public IEnumerable<TicketInvoice> GetAll()
        {
            return _context.TicketInvoices
                .Include(ti => ti.TicketDetails)
                .Include(ti => ti.TicketInvoiceFoodItems)
                .Include(ti => ti.Promotion)
                .AsEnumerable();
        }

        public async Task UpdateAsync(TicketInvoice invoice)
        {
            _context.TicketInvoices.Update(invoice);
        }

        public async Task DeleteAsync(int invoiceId)
        {
            var invoice = await _context.TicketInvoices
                .Include(i => i.TicketDetails)
                .Include(i => i.TicketInvoiceFoodItems)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice != null)
            {
                _context.TicketDetails.RemoveRange(invoice.TicketDetails);
                _context.TicketInvoiceFoodItems.RemoveRange(invoice.TicketInvoiceFoodItems);
                _context.TicketInvoices.Remove(invoice);
            }
        }

        public async Task<List<TicketDetail>> GetTicketDetailsBySeatsAndShowtimeAsync(IEnumerable<int> seatIds, int showtimeInstanceId)
        {
            return await _context.TicketDetails
                .Where(td => seatIds.Contains(td.SeatDataId) && td.ShowtimeInstanceId == showtimeInstanceId)
                .ToListAsync();
        }

        public async Task RemoveTicketDetail(TicketDetail ticketDetail)
        {
            _context.TicketDetails.Remove(ticketDetail);
        }
    }
}