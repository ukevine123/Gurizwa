using Domain.Entities;
using Application.DTO;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PaymentRepository : IPayment
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public PaymentRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // 1. AUTOMATED PENALTY LOGIC: Handles the 5% fee and Debt Reload
        public async Task CreatePaymentWithPenaltyAsync(CreatePaymentDTO paymentDTO, decimal shortfall, decimal penaltyAmount)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // A. Record the actual money received
                var payment = new Payment
                {
                    DisbursementId = paymentDTO.DisbursementId,
                    AccountId = paymentDTO.AccountId,
                    PaymentTypeId = paymentDTO.PaymentTypeId,
                    Amount = paymentDTO.Amount,
                    PaymentDate = paymentDTO.PaymentDate ?? DateTime.Now,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                context.Payments.Add(payment);

                // B. Create the Penalty Record (for the 5% fee)
                // We fetch the LoanApplicationId associated with this Disbursement
                var loanAppId = await context.Disbursements
                    .Where(d => d.Id == paymentDTO.DisbursementId)
                    .Select(d => d.LoanApplicationId)
                    .FirstOrDefaultAsync();

                var penalty = new Penality
                {
                    LoanApplicationId = loanAppId,
                    Amount = penaltyAmount,
                    Date = DateTime.Now,
                    ReasonId = 1, // ID for "Late/Underpayment"
                    Description = $"Auto-Penalty: Shortfall of {shortfall:C} + 5% Fee Applied.",
                    IsActive = true
                };
                context.Penalties.Add(penalty);

                // C. THE DEBT RELOAD: Add unpaid money + penalty back to the loan principal
                var disbursement = await context.Disbursements.FindAsync(paymentDTO.DisbursementId);
                if (disbursement != null)
                {
                    disbursement.Amount += (shortfall + penaltyAmount);
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

        // 2. STANDARD PAYMENT: Used when amount and date are perfect
        public async Task CreatePaymentAsync(CreatePaymentDTO paymentDTO)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var payment = new Payment
            {
                DisbursementId = paymentDTO.DisbursementId,
                AccountId = paymentDTO.AccountId,
                PaymentTypeId = paymentDTO.PaymentTypeId,
                Amount = paymentDTO.Amount,
                PaymentDate = paymentDTO.PaymentDate ?? DateTime.Now,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            context.Payments.Add(payment);
            await context.SaveChangesAsync();
        }

        // 3. SCHEDULER ENGINE: Calculates what is due next for the Service/UI
        public async Task<(decimal Amount, DateTime Date)> GetNextScheduledPaymentAsync(int disbursementId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var disbursement = await context.Disbursements
                .Include(d => d.PaymentModality)
                .FirstOrDefaultAsync(d => d.Id == disbursementId);

            if (disbursement == null) return (0, DateTime.Today);

            var totalPaid = await context.Payments
                .Where(p => p.DisbursementId == disbursementId && p.IsActive)
                .SumAsync(p => p.Amount);

            decimal installmentAmount = disbursement.Amount / disbursement.TotalInstallments;
            string mode = disbursement.PaymentModality?.Mode?.ToLower() ?? "monthly";
            
            for (int i = 1; i <= disbursement.TotalInstallments; i++)
            {
                decimal cumulativeDue = installmentAmount * i;
                
                if (totalPaid < cumulativeDue)
                {
                    DateTime dueDate = mode switch
                    {
                        "daily" => disbursement.StartDate.AddDays(i),
                        "weekly" => disbursement.StartDate.AddDays(i * 7),
                        "monthly" => disbursement.StartDate.AddMonths(i),
                        _ => disbursement.StartDate.AddMonths(i)
                    };

                    decimal paidForPrevious = installmentAmount * (i - 1);
                    decimal paidTowardsCurrent = totalPaid - paidForPrevious;
                    decimal remainingForThis = installmentAmount - (paidTowardsCurrent > 0 ? paidTowardsCurrent : 0);

                    return (remainingForThis, dueDate);
                }
            }
            return (0, DateTime.Today);
        }

        // 4. LISTING: Returns all payments for the Grid
        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Payments
                .Include(i => i.Disbursement)
                    .ThenInclude(d => d.LoanApplication)
                        .ThenInclude(l => l.Borrower)
                .Include(i => i.Account)
                .Include(i => i.PaymentType)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        // 5. LOOKUP: Get single payment details
        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Payments
                .Include(i => i.Disbursement)
                .Include(i => i.Account)
                .Include(i => i.PaymentType)
                .FirstOrDefaultAsync(i => i.Id == id);
        }
    }
}