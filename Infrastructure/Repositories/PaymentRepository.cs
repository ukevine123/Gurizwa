using Domain.Entities;
using Application.DTO;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Domain.ValueObjects;

namespace Infrastructure.Repositories
{
    public class PaymentRepository : IPayment
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IUserContext _userContext;

        public PaymentRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IUserContext userContext)
        {
            _contextFactory = contextFactory;
            _userContext = userContext;
        }

        public async Task CreatePaymentAsync(CreatePaymentDTO paymentDTO)
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
                // 1. Fetch Disbursement
                var disbursement = await context.Disbursements
                
                    .Include(d => d.LoanApplication)
                    .Include(d => d.Payments)
                     .Where(a => a.PersonId == _userContext.Id)
                    .FirstOrDefaultAsync(d => d.Id == paymentDTO.DisbursementId);

                if (disbursement == null) throw new Exception("Disbursement not found.");

                // 2. CHECK: Prevent payment if today is before the Loan Start Date
                // COMMENTED OUT to allow payment before the installment date
                // if (DateTime.Now.Date < disbursement.StartDate.Date)
                // {
                //     throw new InvalidOperationException($"The installment cycle for this loan starts on {disbursement.StartDate:MMMM dd, yyyy}. Please wait for the cycle to begin.");
                // }

                // 3. Calculate Total Remaining Debt for the whole loan
                var totalPaidSoFar = disbursement.Payments
                    .Where(p => p.IsActive)
                    .Sum(p => p.Amount);

                decimal remainingTotalDebt = disbursement.Amount - totalPaidSoFar;

                // 4. Validate payment amount doesn't exceed remaining debt
                if (paymentDTO.Amount > remainingTotalDebt)
                {
                    throw new InvalidOperationException($"Payment amount ({paymentDTO.Amount:N2}) exceeds remaining debt ({remainingTotalDebt:N2}). Please enter a valid amount.");
                }

                // 5. Check if loan is being completed
                bool isLoanCompleted = paymentDTO.Amount >= remainingTotalDebt;

                // 6. Update Account Balance
                var account = await context.Accounts.FirstOrDefaultAsync(a => a.Id == paymentDTO.AccountId);
                if (account == null) throw new Exception("Target account not found.");
                
                account.Balance += paymentDTO.Amount;

                // 7. Create Payment Record
                // Determine payment status: Completed ONLY if remaining debt after payment = 0, Partial otherwise
                decimal remainingDebtAfterPayment = remainingTotalDebt - paymentDTO.Amount;
                string paymentStatus = remainingDebtAfterPayment <= 0 ? "Completed" : "Partial";
                
                var payment = new Payment
                {
                    DisbursementId = paymentDTO.DisbursementId,
                    AccountId = paymentDTO.AccountId,
                    PaymentTypeId = paymentDTO.PaymentTypeId,
                    Amount = paymentDTO.Amount,
                    PaymentDate = DateTime.Now,
                    Status = paymentStatus,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                   PersonId = user.Person.Id,
                };

                context.Payments.Add(payment);
                var applicationCode = disbursement.LoanApplication?.ApplicationCode ?? disbursement.LoanApplicationId.ToString();
                context.ActivityLogs.Add(ActivityLogFactory.Create(
                    _userContext,
                    "Payment Created",
                    nameof(Payment),
                    applicationCode,
                    $"Recorded payment of {paymentDTO.Amount:N2} for loan application {applicationCode}. Remaining debt: {remainingDebtAfterPayment:N2}."));

                // 8. Update Loan Status if fully paid
                if (isLoanCompleted)
                {
                    disbursement.LoanApplication.Status = LoanStatus.Paid;
                    context.ActivityLogs.Add(ActivityLogFactory.Create(
                        _userContext,
                        "Loan Paid",
                        nameof(LoanApplication),
                        applicationCode,
                        $"Loan application {applicationCode} was fully paid."));
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CreatePaymentWithPenaltyAsync(CreatePaymentDTO paymentDTO, decimal shortfall, decimal penaltyAmount)
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
                // 1. Fetch Disbursement + Application + LoanProduct + Setting
                var disbursement = await context.Disbursements
                    .Include(d => d.LoanApplication)
                        .ThenInclude(la => la.LoanProductSetting) // Updated navigation property name
                    
                    .Include(d => d.Payments)
                    .FirstOrDefaultAsync(d => d.Id == paymentDTO.DisbursementId);

                if (disbursement == null) throw new Exception("Disbursement not found.");

                // 2. Record Payment Amount in the target Account
                var account = await context.Accounts.FirstOrDefaultAsync(a => a.Id == paymentDTO.AccountId);
                if (account == null) throw new Exception("Target account not found.");
                account.Balance += paymentDTO.Amount;

                // 3. Get actual penalty rate from Product Settings for the description
                decimal rate = disbursement.LoanApplication?.LoanProductSetting?.PenalityRate ?? 0;

                // 4. Create Penalty Record (Populates the Penalty Page)
                var penalty = new Penality
                {
                    LoanApplicationId = disbursement.LoanApplicationId,
                    Amount = penaltyAmount,
                    Date = DateTime.Now,
                    ReasonId = 1, 
                    Description = $"Penalty ({rate}%). Shortfall: {shortfall:N2}.",
                    IsActive = true
                };
                context.Penalties.Add(penalty);

                // 5. Add Penalty to the Loan Balance (Reflected in Loan Details)
                // We add ONLY the penaltyAmount. The shortfall is already part of the principal.
                disbursement.Amount += penaltyAmount;

                // 6. Create the Payment Record
                // Calculate remaining debt after this payment to determine status
                decimal totalPaidBeforeThisPayment = disbursement.Payments
                    .Where(p => p.IsActive)
                    .Sum(p => (decimal?)p.Amount ?? 0);
                decimal remainingDebtAfterThisPayment = disbursement.Amount - totalPaidBeforeThisPayment - paymentDTO.Amount;
                string paymentStatus = remainingDebtAfterThisPayment <= 0 ? "Completed" : "Partial";

                var payment = new Payment
                {
                    DisbursementId = paymentDTO.DisbursementId,
                    AccountId = paymentDTO.AccountId,
                    PaymentTypeId = paymentDTO.PaymentTypeId,
                    Amount = paymentDTO.Amount,
                    PaymentDate = DateTime.Now,
                    Status = paymentStatus,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    PersonId = user.Person.Id
                };
                context.Payments.Add(payment);
                var applicationCode = disbursement.LoanApplication?.ApplicationCode ?? disbursement.LoanApplicationId.ToString();
                context.ActivityLogs.Add(ActivityLogFactory.Create(
                    _userContext,
                    "Payment With Penalty Created",
                    nameof(Payment),
                    applicationCode,
                    $"Recorded payment of {paymentDTO.Amount:N2} and penalty of {penaltyAmount:N2} for loan application {applicationCode}. Remaining debt: {remainingDebtAfterThisPayment:N2}."));

                // 7. Check if loan is finished
                decimal totalPaidSoFar = disbursement.Payments
                    .Where(p => p.IsActive)
                    .Sum(p => (decimal?)p.Amount ?? 0) + paymentDTO.Amount;

                if (totalPaidSoFar >= disbursement.Amount)
                {
                    disbursement.LoanApplication.Status = LoanStatus.Paid;
                    context.ActivityLogs.Add(ActivityLogFactory.Create(
                        _userContext,
                        "Loan Paid",
                        nameof(LoanApplication),
                        applicationCode,
                        $"Loan application {applicationCode} was fully paid."));
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<(decimal Amount, DateTime Date)> GetNextScheduledPaymentAsync(int disbursementId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var disbursement = await context.Disbursements
                .Include(d => d.PaymentModality)
                .FirstOrDefaultAsync(d => d.Id == disbursementId);

            if (disbursement == null) return (0, DateTime.Today);

            var totalPaid = await context.Payments
                .Where(p => p.DisbursementId == disbursementId && p.IsActive)
                .SumAsync(p => (decimal?)p.Amount ?? 0);

            decimal installmentAmount = disbursement.Amount / disbursement.TotalInstallments;
            string mode = disbursement.PaymentModality?.Mode?.ToLower() ?? "monthly";
            DateTime today = DateTime.Today;

            for (int i = 0; i < disbursement.TotalInstallments; i++)
            {
                DateTime dueDate = mode switch
                {
                    "daily" => disbursement.StartDate.AddDays(i),
                    "weekly" => disbursement.StartDate.AddDays(i * 7),
                    "monthly" => disbursement.StartDate.AddMonths(i),
                    "yearly" => disbursement.StartDate.AddYears(i),
                    _ => disbursement.StartDate.AddMonths(i)
                };

                decimal cumulativeDueSoFar = installmentAmount * (i + 1);

                if (today <= dueDate || totalPaid < cumulativeDueSoFar)
                {
                    decimal paidForPreviousPeriods = installmentAmount * i;
                    decimal paidTowardsCurrent = Math.Max(0, totalPaid - paidForPreviousPeriods);
                    decimal remaining = Math.Max(0, installmentAmount - paidTowardsCurrent);

                    return (remaining, dueDate);
                }
            }
            
            return (0, DateTime.Today);
        }

        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
             if (_userContext.Id == null)
        {
            return new List<Payment>();
        }
            return await context.Payments
                .Where(a => a.PersonId == _userContext.Id)
                .Include(i => i.Disbursement).ThenInclude(d => d.LoanApplication).ThenInclude(l => l.Borrower)
                .Include(i => i.Account)
                .Include(i => i.PaymentType)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
             if (_userContext.Id == null)
            {
                return null;
            }
            return await context.Payments
                .Where(a => a.PersonId == _userContext.Id)
                .Include(i => i.Account)
                .Include(i => i.PaymentType)
                .FirstOrDefaultAsync(i => i.Id == id);
        }
    }
}
