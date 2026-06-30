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
        private readonly IUserContext _userContext;

      public AccountRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory, IUserContext userContext)
        {
            _dbContextFactory = dbContextFactory;
             _userContext = userContext;
        }

      //Retrieving Accounts

      public async Task<List<Account>> GetAllAccountsAsync()
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            // return await dbContext.Accounts
            // .Include(i => i.AccountType)
            // .ToListAsync();


            if (_userContext.Id == null)
            {
                return new List<Account>();
            }

            return await  dbContext.Users
                .Where(u => u.Id == _userContext.Id)
                .SelectMany(u => u.Person.Accounts)
                 .Include(i => i.AccountType)
                  .ToListAsync();
        }

        public async Task<Account> GetAccountByIdAsync(int id)
        {
            //  using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            //  return await dbContext.Accounts
            // .Include(i => i.AccountType)
            // .FirstOrDefaultAsync(i => i.Id == id);
            if (_userContext.Id == null)
    {
        return null;
    }

    using var dbContext = await _dbContextFactory.CreateDbContextAsync();

    // 2. Fetch the account ONLY if it belongs to the logged-in user
    return await dbContext.Accounts
        .Include(a => a.AccountType)
        .Where(a => a.PersonId == _userContext.Id) // Ensure ownership
        .FirstOrDefaultAsync(a => a.Id == id);    // Find the specific item
        }

        public async Task CreateAccountAsync(AccountCreateDTO AccountDTO)
        {
          if (_userContext.Id == null)
            {
                throw new Exception("User not authenticated");
            }

         using var dbContext = await _dbContextFactory.CreateDbContextAsync();

    // 2. Query 'Users' from the dbContext instance, NOT the factory
         var user = await dbContext.Users
        .Include(u => u.Person) // You'll likely need this to link the account
        .FirstOrDefaultAsync(u => u.Id == _userContext.Id);

            if (user == null)
            {
                throw new Exception("User record not found");
            }

            // await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            Account Account = new()
            {
                Name = AccountDTO.Name,
                Provider = AccountDTO.Provider,
                AccountTypeId = AccountDTO.AccountTypeId,
                PersonId = user.PersonId,
                AccountNumber = AccountDTO.AccountNumber,
                Balance = AccountDTO.Balance,
                Currency = AccountDTO.Currency 
                
            };
            dbContext.Accounts.Add(Account);
            await dbContext.SaveChangesAsync();
        }
    

    
    }
    
}
