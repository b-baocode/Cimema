using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;

namespace MV.InfrastructureLayer.Repositories
{
    public class PaymentUpFrontRepository : IPaymentUpFrontRepository
    {
        private readonly MovietheatermanagementContext _context;

        public PaymentUpFrontRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }

        public async Task<PaymentUpFront> AddPaymentUpFrontAsync(PaymentUpFront payment)
        {
            await _context.PaymentUpFronts.AddAsync(payment);
            return payment;
        }

        public async Task<PaymentUpFront> DeletePaymentUpFrontAsync(int id)
        {
            var payment = await _context.PaymentUpFronts.FindAsync(id);
            if (payment != null)
            {
                _context.PaymentUpFronts.Remove(payment);
            }
            return payment;
        }

        public async Task<IEnumerable<PaymentUpFront>> GetAllPaymentUpFrontsAsync()
        {
            return await _context.PaymentUpFronts.ToListAsync();
        }

        public async Task<PaymentUpFront> GetPaymentUpFrontByIdAsync(int id)
        {
            return await _context.PaymentUpFronts.FindAsync(id);
        }

        public async Task<PaymentUpFront> UpdatePaymentUpFrontAsync(PaymentUpFront payment)
        {
            _context.Entry(payment).State = EntityState.Modified;
            return payment;
        }

        public async Task<PaymentUpFront> UndoPaymentUpFrontAsync(int id)
        {
            var payment = await _context.PaymentUpFronts.FindAsync(id);
            if (payment != null)
            {
                payment.Status = "Canceled";
                _context.Entry(payment).State = EntityState.Modified;
            }
            return payment;
        }
    }
} 

