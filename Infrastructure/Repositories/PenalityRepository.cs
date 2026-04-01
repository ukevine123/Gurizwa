using Domain.Entities;
using Application.DTO;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PenalityRepository : IPenality
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public PenalityRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Penality>> GetAllPenalitiesAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Penalties
                .Include(i => i.LoanApplication)
                .ThenInclude(l => l.Borrower) // Including borrower for better UI display
                .Include(i => i.Reason)
                .Where(p => p.IsActive) // Usually you only want active penalties
                .OrderByDescending(p => p.Date)
                .ToListAsync();
        }

        public async Task<Penality?> GetPenalityByIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Penalties
                .Include(i => i.LoanApplication)
                .Include(i => i.Reason)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task CreatePenalityAsync(CreatePenalityDTO penalityDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            
            var penality = new Penality
            {
                LoanApplicationId = penalityDTO.LoanApplicationId,
                Amount = penalityDTO.Amount,
                Date = penalityDTO.Date ?? DateTime.UtcNow,
                ReasonId = penalityDTO.ReasonId,
                Description = penalityDTO.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Penalties.Add(penality);

            // Logic Check: If a penalty is created, we usually update the 
            // main Loan balance (Disbursement.Amount) in the Service Layer.
            await context.SaveChangesAsync();
        }

        public async Task UpdatePenalityAsync(Penality penality)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Penalties.Update(penality);
            await context.SaveChangesAsync();
        }

        public async Task DeletePenalityAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var penality = await context.Penalties.FindAsync(id);
            if (penality != null)
            {
                // Soft delete is safer for financial records
                penality.IsActive = false; 
                await context.SaveChangesAsync();
            }
        }
    }
}