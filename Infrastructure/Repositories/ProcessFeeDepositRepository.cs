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

    public ProcessFeeDepositRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
         _contextFactory = contextFactory;
        }
        public  async Task<List<ProcessFeeDeposit>> GetAllProcessFeeDepositsAsync()
        {
        using var dbContext = await _contextFactory.CreateDbContextAsync();
        return await dbContext.ProcessFeeDeposits
            .Include(a => a.LoanApplication)
            .Include(a => a.PaymentType)
            .Include(a => a.Borrower)
            .Include(a => a.Account)
            .ToListAsync();
        }
        public async Task <ProcessFeeDeposit> GetProcessFeeDepositByIdAsync(int Id)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            return  dbContext.ProcessFeeDeposits.FirstOrDefault(t => t.Id == Id);
        }
         public async Task CreateProcessFeeDepositAsync(CreateProcessFeeDepositDTO processFeeDepositDTO)
        {
              using var dbContext = await _contextFactory.CreateDbContextAsync();
                var processFeeDeposit = await dbContext.ProcessFeeDeposits.FindAsync(processFeeDepositDTO);
                var loanApplication = await dbContext.LoanApplications.FindAsync(processFeeDepositDTO.LoanApplicationId);
                var paymentType = await dbContext.PaymentTypes.FindAsync(processFeeDepositDTO.PaymentTypeId);
                var borrower = await dbContext.Borrowers.FindAsync(processFeeDepositDTO.BorrowerId);
                var account = await dbContext.Accounts.FindAsync(processFeeDepositDTO.AccountId);

          var _processFeeDeposit = new ProcessFeeDeposit
            {
                PaymentType = paymentType,
                LoanApplication = loanApplication,
                Borrower = borrower,
                Amount = processFeeDepositDTO.Amount,
                DepositDate = processFeeDepositDTO.DepositDate,
                Account = account,
                
            };
            dbContext.ProcessFeeDeposits.Add(_processFeeDeposit);
            dbContext.SaveChanges();
        } 
     public async Task UpdateProcessFeeDepositAsync(int Id,UpdateProcessFeeDepositDTO processFeeDepositDTO)
        {
           using var dbContext = await _contextFactory.CreateDbContextAsync();
     
                var _loanApplication = await dbContext.ProcessFeeDeposits
                    .Include(a => a.PaymentType)
                    .Include(a => a.Account)
                    
                    .FirstOrDefaultAsync(t => t.Id == Id);

                if (_loanApplication != null)
                {
                    // Update fields
                    _loanApplication.Amount = processFeeDepositDTO.Amount;
                    _loanApplication.PaymentTypeId = processFeeDepositDTO.PaymentTypeId;
                    _loanApplication.AccountId = processFeeDepositDTO.AccountId;
                    _loanApplication.Status = processFeeDepositDTO.Status; // Assuming DTO uses LoanStatus enum
                    _loanApplication.DepositDate = processFeeDepositDTO.DepositDate;

                    dbContext.ProcessFeeDeposits.Update(_loanApplication);
                    await dbContext.SaveChangesAsync();
               }
        }
               
    }
}
     
    
    
        