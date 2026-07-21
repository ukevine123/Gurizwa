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
        private readonly IEmailService _emailService;

        public DisbursementRepository(
            IDbContextFactory<ApplicationDbContext> contextFactory, 
            IUserContext userContext,
            IEmailService emailService)
        {
            _contextFactory = contextFactory;
            _userContext = userContext;
            _emailService = emailService;
        }

        public async Task<List<Disbursement>> GetAllDisbursementsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
             if (_userContext.Id == null)
        {
            return new List<Disbursement>();
        }
            var allowedPersonIds = await _userContext.GetAllowedPersonIdsAsync();
            return await context.Disbursements
                .Include(i => i.LoanApplication).ThenInclude(l => l.Borrower)
                .Include(i => i.PaymentModality)
                 .Where(a => allowedPersonIds.Contains(a.PersonId))
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
            var allowedPersonIds = await _userContext.GetAllowedPersonIdsAsync();
            var data = await context.Disbursements
                .Include(i => i.LoanApplication).ThenInclude(l => l.Borrower)
                .Include(i => i.PaymentModality)
                .Where(a => allowedPersonIds.Contains(a.PersonId)) 
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
            var allowedPersonIds = await _userContext.GetAllowedPersonIdsAsync();
            return await context.Disbursements
                .Include(i => i.LoanApplication).ThenInclude(l => l.Borrower)
                .Include(i => i.LoanApplication).ThenInclude(l => l.LoanProductSetting)
                .Include(i => i.PaymentModality)
                .Include(i => i.Payments)
                .Where(a => allowedPersonIds.Contains(a.PersonId))
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Disbursement?> GetDisbursementByLoanApplicationIdAsync(int loanApplicationId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
             if (_userContext.Id == null)
            {
                return null;
            }
            var allowedPersonIds = await _userContext.GetAllowedPersonIdsAsync();
            return await context.Disbursements
                .Include(i => i.LoanApplication)
                .Include(i => i.PaymentModality)
                .Where(a => allowedPersonIds.Contains(a.PersonId)) 
                .Include(i => i.Payments)
                .Where(d => d.IsActive)
                .FirstOrDefaultAsync(d => d.LoanApplicationId == loanApplicationId);
        }

        public async Task<CreateDisbursementDTO> PrepareDisbursementFromApplicationAsync(int loanApplicationId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var allowedPersonIds = await _userContext.GetAllowedPersonIdsAsync();
            var app = await context.LoanApplications
                .Include(l => l.LoanProductSetting)
                .Where(a => allowedPersonIds.Contains(a.PersonId)) 
                .FirstOrDefaultAsync(l => l.Id == loanApplicationId);

            if (app == null) return new CreateDisbursementDTO();

            decimal rate = app.LoanProductSetting?.InterestRate ?? 0;
            decimal processingFeePercentage = app.LoanProductSetting?.ProcessingFee ?? 0;
            decimal processingFeeAmount = app.AmountRequested * (processingFeePercentage / 100);

            return new CreateDisbursementDTO
            {
                LoanApplicationId = app.Id,
                PaymentModalityId = app.PaymentModalityId,
                InterestRate = rate,
                ProcessingFeePercentage = processingFeePercentage,
                ProcessingFeeAmount = processingFeeAmount,
                PrincipalOffered = app.AmountRequested,
                TotalInstallments = 1 
            };
        }

        public async Task RescheduleLoanAsync(int oldDisbursementId, decimal totalDebt, int newModeId, int newInstallments, DateTime startDate, decimal interestRate)
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
                    InterestRate = interestRate, 
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
                    .Include(l => l.Borrower)
                    .Include(l => l.Person)
                    .FirstOrDefaultAsync(l => l.Id == disbursementDTO.LoanApplicationId);
                
                if (loanApp == null) throw new InvalidOperationException("Loan Application not found.");

                int autoModalityId = disbursementDTO.PaymentModalityId;
                decimal autoRate = disbursementDTO.InterestRate;
                decimal netPrincipal = disbursementDTO.PrincipalOffered;
                decimal procFeeAmount = disbursementDTO.ProcessingFeeAmount;
                decimal totalInterest = netPrincipal * (autoRate / 100);
                
                decimal disbursedAmount = disbursementDTO.IsPrepayment 
                    ? (netPrincipal - procFeeAmount - totalInterest)
                    : (netPrincipal - procFeeAmount);

                var account = await context.Accounts.FirstOrDefaultAsync(a => a.Id == disbursementDTO.AccountId);
                if (account == null) throw new InvalidOperationException("Source account not found.");
                if (account.Balance < disbursedAmount)
                    throw new InvalidOperationException($"Insufficient Funds! Required: {disbursedAmount:N2}, Balance: {account.Balance:N2}.");

                account.Balance -= disbursedAmount;
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
                    PersonId = loanApp.PersonId,
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

                // Send Confirmation Email asynchronously
                await SendDisbursementEmailAsync(loanApp, disbursementDTO, disbursedAmount, mode);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task SendDisbursementEmailAsync(LoanApplication loanApp, CreateDisbursementDTO dto, decimal netDisbursed, string mode)
        {
            try
            {
                string? recipientEmail = loanApp.Borrower?.Email ?? loanApp.Person?.Email;
                if (string.IsNullOrWhiteSpace(recipientEmail)) return;

                string borrowerName = "";
                if (loanApp.Borrower != null)
                {
                    borrowerName = !string.IsNullOrWhiteSpace(loanApp.Borrower.CompanyName)
                        ? loanApp.Borrower.CompanyName
                        : $"{loanApp.Borrower.FirstName} {loanApp.Borrower.LastName}".Trim();
                }
                if (string.IsNullOrWhiteSpace(borrowerName) && loanApp.Person != null)
                {
                    borrowerName = !string.IsNullOrWhiteSpace(loanApp.Person.CompanyName)
                        ? loanApp.Person.CompanyName
                        : $"{loanApp.Person.FirstName} {loanApp.Person.LastName}".Trim();
                }
                if (string.IsNullOrWhiteSpace(borrowerName)) borrowerName = "Valued Customer";

                string appCode = loanApp.ApplicationCode ?? $"APP-{loanApp.Id}";
                string modalityText = dto.IsPrepayment 
                    ? "Prepayment (Interest Deducted Upfront)" 
                    : "Postpayment (Interest Paid per Installment)";

                int installments = dto.TotalInstallments > 0 ? dto.TotalInstallments : 1;
                decimal installmentAmt = dto.Amount / installments;
                DateTime startDate = dto.StartDate ?? DateTime.Today;

                string subject = $"Loan Disbursed - {appCode}";

                string htmlBody = $@"
<div style=""font-family: 'Segoe UI', Arial, sans-serif; max-width: 650px; margin: 0 auto; border: 1px solid #e0e4f0; border-radius: 12px; overflow: hidden; background-color: #ffffff;"">
    <div style=""background: linear-gradient(135deg, #1B2559 0%, #3F51B5 100%); padding: 30px 20px; text-align: center; color: #ffffff;"">
        <h1 style=""margin: 0; font-size: 24px; font-weight: 700; letter-spacing: 0.5px;"">Guriza</h1>
        <p style=""margin: 6px 0 0 0; font-size: 14px; opacity: 0.9;"">Official Loan Disbursement Advice</p>
    </div>
    
    <div style=""padding: 25px 30px;"">
        <p style=""font-size: 16px; color: #1B2559; margin-top: 0;"">Dear <strong>{borrowerName}</strong>,</p>
        <p style=""font-size: 14px; color: #4A5568; line-height: 1.6;"">
            We are pleased to inform you that your loan application <strong>{appCode}</strong> has been successfully disbursed. Below are the full financial details and repayment terms for your reference.
        </p>

        <div style=""background: #F0FDF4; border: 1px solid #BBF7D0; border-radius: 10px; padding: 20px; text-align: center; margin: 20px 0;"">
            <span style=""font-size: 13px; color: #166534; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px;"">Net Cash Disbursed</span>
            <div style=""font-size: 32px; font-weight: 800; color: #15803D; margin-top: 4px;"">{netDisbursed:N2}</div>
        </div>

        <h3 style=""font-size: 15px; color: #1B2559; border-bottom: 2px solid #E2E8F0; padding-bottom: 8px; margin-top: 25px;"">Disbursement Details</h3>
        <table style=""width: 100%; border-collapse: collapse; font-size: 14px; margin-bottom: 20px;"">
            <tr style=""border-bottom: 1px solid #EDF2F7;"">
                <td style=""padding: 10px 0; color: #718096;"">Application Code:</td>
                <td style=""padding: 10px 0; color: #1B2559; font-weight: 700; text-align: right;"">{appCode}</td>
            </tr>
            <tr style=""border-bottom: 1px solid #EDF2F7;"">
                <td style=""padding: 10px 0; color: #718096;"">Approved Principal:</td>
                <td style=""padding: 10px 0; color: #1B2559; font-weight: 600; text-align: right;"">{dto.PrincipalOffered:N2}</td>
            </tr>
            <tr style=""border-bottom: 1px solid #EDF2F7;"">
                <td style=""padding: 10px 0; color: #718096;"">Processing Fee ({dto.ProcessingFeePercentage:N1}%):</td>
                <td style=""padding: 10px 0; color: #E53E3E; font-weight: 600; text-align: right;"">- {dto.ProcessingFeeAmount:N2}</td>
            </tr>
            <tr style=""border-bottom: 1px solid #EDF2F7;"">
                <td style=""padding: 10px 0; color: #718096;"">Interest Rate (%):</td>
                <td style=""padding: 10px 0; color: #1B2559; font-weight: 600; text-align: right;"">{dto.InterestRate:N1}%</td>
            </tr>
            <tr style=""border-bottom: 1px solid #EDF2F7;"">
                <td style=""padding: 10px 0; color: #718096;"">Interest Deduction Modality:</td>
                <td style=""padding: 10px 0; color: #2B6CB0; font-weight: 700; text-align: right;"">{modalityText}</td>
            </tr>
            <tr style=""border-bottom: 1px solid #EDF2F7;"">
                <td style=""padding: 10px 0; color: #718096;"">Net Cash Disbursed:</td>
                <td style=""padding: 10px 0; color: #2F855A; font-weight: 700; text-align: right;"">{netDisbursed:N2}</td>
            </tr>
            <tr>
                <td style=""padding: 10px 0; color: #718096;"">Total Repayable Amount:</td>
                <td style=""padding: 10px 0; color: #1B2559; font-weight: 800; font-size: 16px; text-align: right;"">{dto.Amount:N2}</td>
            </tr>
        </table>

        <h3 style=""font-size: 15px; color: #1B2559; border-bottom: 2px solid #E2E8F0; padding-bottom: 8px; margin-top: 25px;"">Repayment Schedule Summary</h3>
        <table style=""width: 100%; border-collapse: collapse; font-size: 14px;"">
            <tr style=""border-bottom: 1px solid #EDF2F7;"">
                <td style=""padding: 10px 0; color: #718096;"">Payment Frequency:</td>
                <td style=""padding: 10px 0; color: #1B2559; font-weight: 600; text-align: right; text-transform: capitalize;"">{mode}</td>
            </tr>
            <tr style=""border-bottom: 1px solid #EDF2F7;"">
                <td style=""padding: 10px 0; color: #718096;"">Total Installments:</td>
                <td style=""padding: 10px 0; color: #1B2559; font-weight: 600; text-align: right;"">{installments} Payment(s)</td>
            </tr>
            <tr style=""border-bottom: 1px solid #EDF2F7;"">
                <td style=""padding: 10px 0; color: #718096;"">Installment Amount:</td>
                <td style=""padding: 10px 0; color: #3182CE; font-weight: 700; text-align: right;"">{installmentAmt:N2} / payment</td>
            </tr>
            <tr>
                <td style=""padding: 10px 0; color: #718096;"">First Repayment Date:</td>
                <td style=""padding: 10px 0; color: #1B2559; font-weight: 600; text-align: right;"">{startDate:yyyy-MM-dd}</td>
            </tr>
        </table>

        <div style=""margin-top: 30px; padding: 15px; background: #F7FAFC; border: 1px solid #E2E8F0; border-radius: 8px; font-size: 13px; color: #4A5568; line-height: 1.5;"">
            <strong>Important Note:</strong> Please ensure your payment account is funded prior to each due date to avoid penalty charges.
        </div>
    </div>

    <div style=""background-color: #1B2559; padding: 15px 20px; text-align: center; font-size: 12px; color: #ffffff; opacity: 0.9;"">
        &copy; {DateTime.UtcNow.Year} Guriza. All rights reserved.
    </div>
</div>";

                await _emailService.SendEmailAsync(recipientEmail, subject, htmlBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DisbursementRepository] Email notification error: {ex.Message}");
            }
        }
    }
}
