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
        private readonly IUserContext _userContext;

        public PaymentTypeRepository(IDbContextFactory<ApplicationDbContext> contextFactory,IUserContext userContext)
        {
            _contextFactory = contextFactory;
            _userContext = userContext;
        }

        public async Task<List<PaymentType>> GetAllPaymentTypesAsync()
        {
             if (_userContext.Id == null)
            {
                return new List<PaymentType>();
            }
            using var context = _contextFactory.CreateDbContext();
            return await context.PaymentTypes
            .Where(a => a.PersonId == _userContext.PersonId)
            .ToListAsync();
        }

        public async Task<PaymentType?> GetPaymentTypeByIdAsync(int id)
        {
             if (_userContext.Id == null)
            {
                return new PaymentType();
            }
            using var context = _contextFactory.CreateDbContext();
            return await context.PaymentTypes
            .Where(a => a.PersonId == _userContext.PersonId) 
            .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task CreatePaymentTypeAsync(CreatePaymentTypeDTO paymentTypeDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            
             if (_userContext.Id == null)
            {
                throw new Exception("User not authenticated");
            }
            var user = await context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Id == _userContext.Id);
            
            if (user == null || user.Person == null)
            {
                throw new Exception("User or Person not found.");
            }
            
            var paymentType = new PaymentType
            {
                PaymentTypeName = paymentTypeDTO.PaymentTypeName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PersonId = user.Person.Id
            };

            context.PaymentTypes.Add(paymentType);
            await context.SaveChangesAsync();
        }
    }
}