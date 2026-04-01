using Domain.Entities;
using Application.DTO;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PaymentTypeRepository : IPaymentType
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public PaymentTypeRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<PaymentType>> GetAllPaymentTypesAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.PaymentTypes.ToListAsync();
        }

        public async Task<PaymentType?> GetPaymentTypeByIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.PaymentTypes.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task CreatePaymentTypeAsync(CreatePaymentTypeDTO paymentTypeDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            
            var paymentType = new PaymentType
            {
                PaymentTypeName = paymentTypeDTO.PaymentTypeName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.PaymentTypes.Add(paymentType);
            await context.SaveChangesAsync();
        }
    }
}