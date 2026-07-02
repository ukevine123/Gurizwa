using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Application.DTO;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories
{
    public class ProcessFeeDepositRepository : IProcessFeeDeposit
    {
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
     private readonly IUserContext _userContext;

    public ProcessFeeDepositRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IUserContext userContext)
        {
         _contextFactory = contextFactory;
         _userContext = userContext;
        }
        public  async Task<List<ProcessFeeDeposit>> GetAllProcessFeeDepositsAsync()
        {
        using var dbContext = await _contextFactory.CreateDbContextAsync();
        if (_userContext.Id == null)
        {
            return new List<ProcessFeeDeposit>();
        }
           return await dbContext.ProcessFeeDeposits
            .Include(a => a.LoanApplication)
            .Where(a => a.PersonId == _userContext.PersonId)
            .Include(a => a.Account)
            .ToListAsync();
        }
        public async Task <ProcessFeeDeposit> GetProcessFeeDepositByIdAsync(int Id)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
             if (_userContext.Id == null)
            {
                return null;
            }
            return  dbContext.ProcessFeeDeposits
            .Where(a => a.PersonId == _userContext.PersonId)
            .FirstOrDefault(t => t.Id == Id);
        }
         public async Task CreateProcessFeeDepositAsync(CreateProcessFeeDepositDTO processFeeDepositDTO)
        {

             if (_userContext.Id == null)
            {
                throw new Exception("User not authenticated");
            }

            using var dbContext = await _contextFactory.CreateDbContextAsync();

    // 2. Query 'Users' from the dbContext instance, NOT the factory
         var user = await dbContext.Users
        .Include(u => u.Person) // You'll likely need this to link the account
        .FirstOrDefaultAsync(u => u.Id == _userContext.Id);

            if (user == null)
            {
                throw new Exception("User record not found");
            }

            if (user.Person == null)
            {
                throw new Exception("Authenticated user does not have an associated Person record.");
            }
             
                // var processFeeDeposit = await dbContext.ProcessFeeDeposits.FindAsync(processFeeDepositDTO);
                var loanApplication = await dbContext.LoanApplications.FindAsync(processFeeDepositDTO.LoanApplicationId);
                var account = await dbContext.Accounts.FindAsync(processFeeDepositDTO.AccountId);

          var _processFeeDeposit = new ProcessFeeDeposit
            {
               
                LoanApplication = loanApplication,
                Amount = processFeeDepositDTO.Amount,
                DepositDate = DateTime.Now,
                Status = processFeeDepositDTO.Status,
                Account = account,
                PersonId = user.Person.Id,

            };
            dbContext.ProcessFeeDeposits.Add(_processFeeDeposit);
            dbContext.SaveChanges();
        } 
     public async Task UpdateProcessFeeDepositAsync(int Id,UpdateProcessFeeDepositDTO processFeeDepositDTO)
        {
           using var dbContext = await _contextFactory.CreateDbContextAsync();
     
                var _loanApplication = await dbContext.ProcessFeeDeposits
                    
                    .Include(a => a.Account)
                    
                    .FirstOrDefaultAsync(t => t.Id == Id);

                if (_loanApplication != null)
                {
                    // Update fields
                    _loanApplication.Amount = processFeeDepositDTO.Amount;
                    _loanApplication.AccountId = processFeeDepositDTO.AccountId;
                    _loanApplication.Status = processFeeDepositDTO.Status; // Assuming DTO uses LoanStatus enum
                    _loanApplication.DepositDate = DateTime.Now; // Update to current date

                    dbContext.ProcessFeeDeposits.Update(_loanApplication);
                    await dbContext.SaveChangesAsync();
               }
        }
               
    }
}
     
    
    
        