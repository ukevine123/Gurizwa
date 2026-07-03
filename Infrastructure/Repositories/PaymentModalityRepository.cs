using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Application.DTO;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PaymentModalityRepository : IPaymentModality
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public PaymentModalityRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<PaymentModality>> GetAllPaymentModalitysAsync()
        {
            // Use a fresh context for each call to avoid concurrency issues
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            return await dbContext.PaymentModalities.ToListAsync();
        }

        public async Task<PaymentModality?> GetPaymentModalityById(int Id)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            // Changed to FirstOrDefaultAsync to maintain a fully async flow
            return await dbContext.PaymentModalities.FirstOrDefaultAsync(t => t.Id == Id);
        }

        public async Task CreatePaymentModality(CreatePaymentModalityDTO paymentModalityDTO)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var paymentModality = new PaymentModality
            {
                Mode = paymentModalityDTO.Mode,
                CreatedBy = "System Manager" // Uncomment if you have a status field here too
            };

            await dbContext.PaymentModalities.AddAsync(paymentModality);
            await dbContext.SaveChangesAsync();
        }
    }
}