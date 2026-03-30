using Application.DTO;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories 
{
    public class AccountTypeRepository : IAccountType
    {
      private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

      public AccountTypeRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

      //Retrieving AccountTypes

      public async Task<List<AccountType>> GetAllAccountTypesAsync()
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            return await dbContext.AccountTypes.ToListAsync();
        }

        public async Task<AccountType?> GetAccountTypeByIdAsync(int id)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            return await dbContext.AccountTypes.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task CreateAccountTypeAsync(AccountTypeCreateDTO AccountTypeDTO)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            AccountType AccountType = new()
            {
                AccountTypeName = AccountTypeDTO.AccountTypeName,
              
                                
            };
            dbContext.AccountTypes.Add(AccountType);
            await dbContext.SaveChangesAsync();
        }
    

    
    }
    
}
