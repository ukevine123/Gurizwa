using Domain.Entities;
using Infrastructure.Data;
using Application.DTO;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BorrowerTypeRepository : IBorrowerType
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public BorrowerTypeRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<BorrowerType>> GetAllBorrowerTypeAsync()
        {
            // Create a fresh context for this specific operation
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            return await dbContext.BorrowerTypes.ToListAsync();
        }

        public async Task<BorrowerType?> GetBorrowerTypeById(int Id)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            return await dbContext.BorrowerTypes.FirstOrDefaultAsync(c => c.Id == Id);
        }

        public async Task CreateBorrowerType(CreateBorrowTypeDTO borrowerTypeDTO)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var borrowerType = new BorrowerType
            {
                Type = borrowerTypeDTO.Type,
                Status = "Active",
            };
            await dbContext.BorrowerTypes.AddAsync(borrowerType);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateBorrowerType(int Id, UpdateBorrowTypeDTO borrowerTypeDTO)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var borrowerType = await dbContext.BorrowerTypes.FindAsync(Id);
            if (borrowerType != null)
            {
                borrowerType.Type = borrowerTypeDTO.Type;
                await dbContext.SaveChangesAsync();
            }
        }
    }
}