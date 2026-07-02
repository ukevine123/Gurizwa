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
        private readonly IUserContext _userContext;

        public BorrowerTypeRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IUserContext userContext)
        {
            _contextFactory = contextFactory;
            _userContext = userContext;
        }

        public async Task<List<BorrowerType>> GetAllBorrowerTypeAsync()
        {
            // Create a fresh context for this specific operation
            if (_userContext.Id == null)
            {
                return new List<BorrowerType>();
            }
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            return await dbContext.BorrowerTypes
            .Where(a => a.PersonId == _userContext.PersonId) 
            .ToListAsync();
        }

        public async Task<BorrowerType?> GetBorrowerTypeById(int Id)
        {
            if (_userContext.Id == null)
            {
                return new BorrowerType();
            }
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            return await dbContext.BorrowerTypes        
            .FirstOrDefaultAsync(c => c.Id == Id);
        }

        public async Task CreateBorrowerType(CreateBorrowTypeDTO borrowerTypeDTO)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            
            if (_userContext.Id == null) throw new Exception("User not authenticated");
            var user = await dbContext.Users.Include(u => u.Person).FirstOrDefaultAsync(u => u.Id == _userContext.Id);
            if (user == null || user.Person == null) throw new Exception("User or Person not found.");

            var borrowerType = new BorrowerType
            {
                Type = borrowerTypeDTO.Type,
                Status = "Active",
                PersonId = user.Person.Id
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