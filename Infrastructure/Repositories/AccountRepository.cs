using Application.DTO;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories 
{
    public class AccountRepository : IAccount
    {
      private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

      public AccountRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

      //Retrieving Accounts

      public async Task<List<Account>> GetAllAccountsAsync()
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            return await dbContext.Accounts
            .Include(i => i.AccountType)
            .ToListAsync();
        }

        public async Task<Account?> GetAccountByIdAsync(int id)
        {
             using var dbContext = await _dbContextFactory.CreateDbContextAsync();
             return await dbContext.Accounts
            .Include(i => i.AccountType)
            .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task CreateAccountAsync(AccountCreateDTO AccountDTO)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            Account Account = new()
            {
                Name = AccountDTO.Name,
                Provider = AccountDTO.Provider,
                AccountTypeId = AccountDTO.AccountTypeId,
                AccountNumber = AccountDTO.AccountNumber,
                Balance = AccountDTO.Balance,
                Currency = AccountDTO.Currency 
                
            };
            dbContext.Accounts.Add(Account);
            await dbContext.SaveChangesAsync();
        }
    

    
    }
    
}
