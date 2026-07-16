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
        private readonly IUserContext _userContext;

        public PaymentModalityRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IUserContext userContext)
        {
            _contextFactory = contextFactory;
            _userContext = userContext;                 
        }

        public async Task<List<PaymentModality>> GetAllPaymentModalitysAsync()
        {
            // Use a fresh context for each call to avoid concurrency issues
            if (_userContext.Id == null)
            {
                return new List<PaymentModality>();
            }
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var settingsPersonId = await _userContext.GetSettingsPersonIdAsync();
            return await dbContext.PaymentModalities
            .Where(a => a.PersonId == settingsPersonId) 
            .ToListAsync();
        }

        public async Task<PaymentModality?> GetPaymentModalityById(int Id)
        {
            if (_userContext.Id == null)
            {
                return new PaymentModality();
            }
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var settingsPersonId = await _userContext.GetSettingsPersonIdAsync();
            // Changed to FirstOrDefaultAsync to maintain a fully async flow
            return await dbContext.PaymentModalities
            .Where(a => a.PersonId == settingsPersonId) 
            .FirstOrDefaultAsync(t => t.Id == Id);
        }

        public async Task CreatePaymentModality(CreatePaymentModalityDTO paymentModalityDTO)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            
            if (_userContext.Id == null) throw new Exception("User not authenticated");
            var user = await dbContext.Users.Include(u => u.Person).FirstOrDefaultAsync(u => u.Id == _userContext.Id);
            if (user == null || user.Person == null) throw new Exception("User or Person not found.");

            var paymentModality = new PaymentModality
            {
                Mode = paymentModalityDTO.Mode,
                CreatedBy = "System Manager",   // Uncomment if you have a status field here too
                PersonId = user.Person.Id
            };

            await dbContext.PaymentModalities.AddAsync(paymentModality);
            await dbContext.SaveChangesAsync();
        }
    }
}