using Domain.Entities;
using Application.DTO;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ReasonRepository : IReason
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public ReasonRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Reason>> GetAllReasonsAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Reasons.ToListAsync();
        }

        public async Task<Reason?> GetReasonByIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Reasons.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task CreateReasonAsync(CreateReasonDTO reasonDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            
            var reason = new Reason
            {
                Name = reasonDTO.Name,
                IsActive = true
            };

            context.Reasons.Add(reason);
            await context.SaveChangesAsync();
        }
    }
}