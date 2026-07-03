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
      private readonly IUserContext _userContext;   

      public AccountTypeRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory, IUserContext userContext)
        {
            _dbContextFactory = dbContextFactory;
            _userContext = userContext;
        }

      //Retrieving AccountTypes

      public async Task<List<AccountType>> GetAllAccountTypesAsync()
        {
            if (_userContext.Id == null)
            {
                return new List<AccountType>();
            } 
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            return await dbContext.AccountTypes
            .Where(a => a.PersonId == _userContext.PersonId) 
            .ToListAsync();
        }

        public async Task<AccountType?> GetAccountTypeByIdAsync(int id)
        {
            if (_userContext.Id == null)
            {
                return new AccountType();
            }
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            return await dbContext.AccountTypes.FirstOrDefaultAsync(c => c.Id == id);
        }
                    
        public async Task CreateAccountTypeAsync(AccountTypeCreateDTO AccountTypeDTO)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            
            if (_userContext.Id == null) throw new Exception("User not authenticated");
            var user = await dbContext.Users.Include(u => u.Person).FirstOrDefaultAsync(u => u.Id == _userContext.Id);
            if (user == null || user.Person == null) throw new Exception("User or Person not found.");

            AccountType AccountType = new()
            {
                AccountTypeName = AccountTypeDTO.AccountTypeName,
                PersonId = user.Person.Id
            };
            dbContext.AccountTypes.Add(AccountType);
            await dbContext.SaveChangesAsync();
        }
    

    
    }
    
}
