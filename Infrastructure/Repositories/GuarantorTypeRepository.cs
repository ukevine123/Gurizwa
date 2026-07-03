using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Application.DTO;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class GuarantorTypeRepository : IGuarantorType
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public GuarantorTypeRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<GuarantorType>> GetAllGuarantorTypesAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.GuarantorTypes.ToListAsync();
        }

        public async Task<GuarantorType?> GetGuarantorTypeById(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            // Changed to FirstOrDefaultAsync to remain truly async
            return await context.GuarantorTypes.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task CreateGuarantorType(CreateGuarantorTypeDTO guarantorTypeDTO)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var guarantorType = new GuarantorType
            {
                Name = guarantorTypeDTO.Name,
                Status = "Active",
                UpdatedBy = "System",             
            };

            context.GuarantorTypes.Add(guarantorType);
            await context.SaveChangesAsync(); // Use SaveChangesAsync here
        }
    }
}