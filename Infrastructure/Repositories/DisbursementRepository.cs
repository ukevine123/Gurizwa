using Domain.Entities;
using Application.DTO;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Domain.ValueObjects;

namespace Infrastructure.Repositories
{
    public class DisbursementRepository : IDisbursement
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IUserContext _userContext;

        public DisbursementRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IUserContext userContext)
        {
            _contextFactory = contextFactory;
            _userContext = userContext;
        }

        public async Task<List<Disbursement>> GetAllDisbursementsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
             if (_userContext.Id == null)
        {
            return new List<Disbursement>();
        }
            return await context.Disbursements
                .Include(i => i.LoanApplication).ThenInclude(l => l.Borrower)
                .Include(i => i.PaymentModality)
                 .Where(a => a.PersonId == _userContext.PersonId)
                .Include(i => i.Payments)
                .Where(d => d.IsActive)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Disbursement>> GetDisbursementsWithBalanceAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
             if (_userContext.Id == null)
            {
                return null;
            }
            var data = await context.Disbursements
                .Include(i => i.LoanApplication).ThenInclude(l => l.Borrower)
                .Include(i => i.PaymentModality)
                .Where(a => a.PersonId == _userContext.PersonId) 
                .Include(i => i.Payments)
                .Where(d => d.IsActive)
                .ToListAsync();

            return data.Select(d =>
            {
                decimal totalPaid = d.Payments?.Where(p => p.IsActive).Sum(p => (decimal?)p.Amount ?? 0) ?? 0;
                d.Amount = d.Amount - totalPaid;
                return d;
            })
            .Where(d => d.Amount > 0.1m)
            .ToList();
        }

        public async Task<Disbursement?> GetDisbursementByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
             if (_userContext.Id == null)
            {
                return null;
            }
            return await context.Disbursements
                .Include(i => i.LoanApplication).ThenInclude(l => l.Borrower)
                .Include(i => i.PaymentModality)
                .Include(i => i.Payments)
                .Where(a => a.PersonId == _userContext.PersonId)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Disbursement?> GetDisbursementByLoanApplicationIdAsync(int loanApplicationId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
             if (_userContext.Id == null)
            {
                return null;
            }
            return await context.Disbursements
                .Include(i => i.LoanApplication)
                .Include(i => i.PaymentModality)
                .Where(a => a.PersonId == _userContext.PersonId) 
                .Include(i => i.Payments)
                .Where(d => d.IsActive)
                .FirstOrDefaultAsync(d => d.LoanApplicationId == loanApplicationId);
        }

        public async Task<CreateDisbursementDTO> PrepareDisbursementFromApplicationAsync(int loanApplicationId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
        
            var app = await context.LoanApplications
                .Include(l => l.LoanProductSetting)
                .Where(a => a.PersonId == _userContext.PersonId) 
                .FirstOrDefaultAsync(l => l.Id == loanApplicationId);

            if (app == null) return new CreateDisbursementDTO();

            decimal feeDeposited = await context.ProcessFeeDeposits
                .Where(p => p.LoanApplicationId == loanApplicationId)
                .SumAsync(p => p.Amount);

            decimal rate = app.LoanProductSetting?.InterestRate ?? 0;

            return new CreateDisbursementDTO
            {
                LoanApplicationId = app.Id,
                PaymentModalityId = app.PaymentModalityId,
                InterestRate = rate,
                PrincipalOffered = app.AmountRequested,
                TotalInstallments = 1 
            };
        }

        public async Task RescheduleLoanAsync(int oldDisbursementId, decimal totalDebt, int newModeId, int newInstallments, DateTime startDate)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // 1. Fetch and Update the Old Disbursement and its Application
                var oldDisb = await context.Disbursements
                    .Include(d => d.LoanApplication)
                    .FirstOrDefaultAsync(d => d.Id == oldDisbursementId);
                
                if (oldDisb == null) throw new InvalidOperationException("Original disbursement not found.");

                // Update Status to Rescheduled
                if (oldDisb.LoanApplication != null)
                {
                    oldDisb.LoanApplication.Status = LoanStatus.Rescheduled;
                }

                // Close the old loan record
                oldDisb.IsActive = false;
                oldDisb.UpdatedAt = DateTime.UtcNow;

                // 2. Prepare the New Disbursement details
                var modality = await context.PaymentModalities.FirstOrDefaultAsync(m => m.Id == newModeId);
                string mode = modality?.Mode?.ToLower() ?? "monthly";
                
                DateTime fixedStartDate = startDate.Date.AddHours(12);
                int n = newInstallments > 0 ? newInstallments : 1;
                DateTime calculatedEndDate = mode switch
                {
                    "daily" => fixedStartDate.AddDays(n),
                    "weekly" => fixedStartDate.AddDays(n * 7),
                    "monthly" => fixedStartDate.AddMonths(n),
                    "yearly" => fixedStartDate.AddYears(n),
                    _ => fixedStartDate.AddMonths(n)
                };

                // 3. Create the New Disbursement entity
                var newDisb = new Disbursement
                {
                    LoanApplicationId = oldDisb.LoanApplicationId,
                    PaymentModalityId = newModeId,
                    AccountId = oldDisb.AccountId,
                    PersonId = oldDisb.PersonId,
                    PrincipalOffered = totalDebt, 
                    InterestRate = 0, 
                    TotalInstallments = n,
                    Amount = totalDebt,
                    StartDate = fixedStartDate,
                    EndDate = calculatedEndDate,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Disbursements.Add(newDisb);
                context.ActivityLogs.Add(ActivityLogFactory.Create(
                    _userContext,
                    "Loan Rescheduled",
                    nameof(Disbursement),
                    oldDisb.LoanApplication?.ApplicationCode ?? oldDisb.LoanApplicationId.ToString(),
                    $"Rescheduled loan {oldDisb.LoanApplication?.ApplicationCode ?? oldDisb.LoanApplicationId.ToString()} with new debt {totalDebt:N2} over {n} installment(s)."));
                
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CreateDisbursementAsync(CreateDisbursementDTO disbursementDTO)
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
          
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try 
            {
                var loanApp = await context.LoanApplications
                    .Include(l => l.LoanProductSetting)
                    .FirstOrDefaultAsync(l => l.Id == disbursementDTO.LoanApplicationId);
                
                if (loanApp == null) throw new InvalidOperationException("Loan Application not found.");

                int autoModalityId = disbursementDTO.PaymentModalityId;
                decimal autoRate = disbursementDTO.InterestRate;
                decimal netPrincipal = disbursementDTO.PrincipalOffered;

                var account = await context.Accounts.FirstOrDefaultAsync(a => a.Id == disbursementDTO.AccountId);
                if (account == null) throw new InvalidOperationException("Source account not found.");
                if (account.Balance < netPrincipal)
                    throw new InvalidOperationException($"Insufficient Funds! Required: {netPrincipal:N2}, Balance: {account.Balance:N2}.");

                account.Balance -= netPrincipal;
                loanApp.Status = LoanStatus.Disbursed; 

                var modality = await context.PaymentModalities.FirstOrDefaultAsync(m => m.Id == autoModalityId);
                string mode = modality?.Mode?.ToLower() ?? "monthly";

                DateTime baseDate = disbursementDTO.StartDate ?? DateTime.Today;
                DateTime startDate = baseDate.Date.AddHours(12);

                int n = disbursementDTO.TotalInstallments > 0 ? disbursementDTO.TotalInstallments : 1;

                decimal totalAmount = disbursementDTO.Amount;

                DateTime calculatedEndDate = mode switch
                {
                    "daily" => startDate.AddDays(n),
                    "weekly" => startDate.AddDays(n * 7),
                    "monthly" => startDate.AddMonths(n),
                    "yearly" => startDate.AddYears(n),
                    _ => startDate.AddMonths(n)
                };

                var disbursement = new Disbursement
                {
                    LoanApplicationId = loanApp.Id,
                    PaymentModalityId = autoModalityId,
                    AccountId = disbursementDTO.AccountId, 
                    PrincipalOffered = netPrincipal,
                    InterestRate = autoRate,
                    TotalInstallments = n,
                    Amount = totalAmount,
                    StartDate = startDate,
                    EndDate = calculatedEndDate,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    PersonId = user.Person.Id,
                };

                context.Disbursements.Add(disbursement);
                context.ActivityLogs.Add(ActivityLogFactory.Create(
                    _userContext,
                    "Loan Disbursed",
                    nameof(Disbursement),
                    loanApp.ApplicationCode ?? loanApp.Id.ToString(),
                    $"Disbursed {netPrincipal:N2} for loan application {loanApp.ApplicationCode ?? loanApp.Id.ToString()}."));

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
