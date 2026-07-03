using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Application.DTO;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories
{
    public class LoanApplicationRepository : ILoanApplication
    {
        // private readonly ApplicationDbContext dbContext;
        // public LoanApplicationRepository(ApplicationDbContext context)
 private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
     private readonly IUserContext _userContext;
    public LoanApplicationRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IUserContext userContext)
        {
        //    dbContext=context; 
         _contextFactory = contextFactory;
         _userContext = userContext;
        }
        public  async Task<List<LoanApplication>> GetAllLoanApplicationsAsync()
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            if (_userContext.Id == null)
            {
                return new List<LoanApplication>();
            }
            return await dbContext.LoanApplications
                .Include(a => a.LoanProductSetting)
                .Include(a => a.Borrower)
                .Include(a => a.PaymentModality)
                .Where(a => a.PersonId == _userContext.PersonId)
                .ToListAsync();
        }
        public async Task <LoanApplication> GetLoanApplicationById(int Id)
        {
            if (_userContext.Id == null)
            {
                return null;
            }
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            return await dbContext.LoanApplications
                .Include(a => a.LoanProductSetting)
                .Include(a => a.Borrower)
                .Include(a => a.PaymentModality)
                .Where(a => a.PersonId == _userContext.PersonId)
                .FirstOrDefaultAsync(a => a.Id == Id); 
        }
         public async Task CreateLoanApplication(CreateApplicationDTO loanApplicationDTO)
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

        var borrower = await dbContext.Borrowers.FindAsync(loanApplicationDTO.BorrowerId);
        var loanProductSetting = await dbContext.LoanProductSettings.FindAsync(loanApplicationDTO.LoanProductSettingId);
        var paymentModality = await dbContext.PaymentModalities.FindAsync(loanApplicationDTO.PaymentModalityId);

        if (borrower == null || loanProductSetting == null || paymentModality == null)
        {
            throw new Exception("One or more related entities required for loan application creation were not found.");
        }

        var generatedCode = $"LN-{DateTime.Now.Year}-{borrower.FirstName}-{borrower.LastName}";
        var currentUserName = string.IsNullOrWhiteSpace(_userContext.FullName) ? _userContext.Email : _userContext.FullName;
        
            var _loanApplication = new LoanApplication
            {
                ApplicationCode = generatedCode,
                BorrowerId = borrower.Id,
                PersonId = user.Person.Id,
                LoanProductSettingId = loanProductSetting.Id,
                PaymentModalityId = paymentModality.Id,
                AmountRequested = loanApplicationDTO.AmountRequested,
                DateofApplication = DateTime.Now,
                Status = LoanStatus.Applied,
                PreferredDate = DateTime.Now,
                ApprovedBy = currentUserName,
                CreatedBy = currentUserName
            };

            dbContext.LoanApplications.Add(_loanApplication);
            dbContext.ActivityLogs.Add(ActivityLogFactory.Create(
                _userContext,
                "Loan Application Created",
                nameof(LoanApplication),
                generatedCode,
                $"Created loan application {generatedCode} for {borrower.FirstName} {borrower.LastName}."));
            


            await dbContext.SaveChangesAsync();
        }
        public async Task UpdateLoanApplication(int Id, UpdateApplicationDTO loanApplicationDTO)
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();
                
                var _loanApplication = await dbContext.LoanApplications
                    .Include(a => a.LoanProductSetting)
                    .FirstOrDefaultAsync(t => t.Id == Id);

                if (_loanApplication != null)
                {
                    var oldStatus = _loanApplication.Status;
                    var oldAmount = _loanApplication.AmountRequested;

                    // Update fields
                    _loanApplication.PaymentModalityId = loanApplicationDTO.PaymentModalityId;
                    _loanApplication.AmountRequested = loanApplicationDTO.AmountRequested;
                    _loanApplication.ApprovedBy = loanApplicationDTO.ApprovedBy;
                    _loanApplication.Status = loanApplicationDTO.Status; // Assuming DTO uses LoanStatus enum
                    _loanApplication.PreferredDate = loanApplicationDTO.PreferredDate;
                    _loanApplication.DateofApplication = loanApplicationDTO.DateofApplication;

                    dbContext.LoanApplications.Update(_loanApplication);
                    dbContext.ActivityLogs.Add(ActivityLogFactory.Create(
                        _userContext,
                        "Loan Application Edited",
                        nameof(LoanApplication),
                        _loanApplication.ApplicationCode,
                        $"Edited loan application {_loanApplication.ApplicationCode}. Amount {oldAmount:N2} to {_loanApplication.AmountRequested:N2}; status {oldStatus} to {_loanApplication.Status}."));


                    await dbContext.SaveChangesAsync();
               }
           }

public async Task<List<LoanApplication>> GetFilteredLoansAsync(string role, int? currentUserId = null)
{
    using var dbContext = await _contextFactory.CreateDbContextAsync();

    var query = dbContext.LoanApplications
        .Include(a => a.LoanProductSetting)
        .Include(a => a.Borrower)
        .AsQueryable();

    if (role == "LoanManager")
    {
        // Use the Enum directly here
        query = query.Where(l => l.Status == LoanStatus.Applied);
    }
    else if (role != "Admin" && currentUserId.HasValue)
    {
        query = query.Where(l => l.BorrowerId == currentUserId.Value);
    }

    return await query.ToListAsync();
}

public async Task UpdateStatusAsync(int id, LoanStatus newStatus)
{
    using var dbContext = await _contextFactory.CreateDbContextAsync();

    var loan = await dbContext.LoanApplications.FindAsync(id);
    if (loan != null)
    {
        var oldStatus = loan.Status;
        loan.Status = newStatus;

        dbContext.ActivityLogs.Add(ActivityLogFactory.Create(
            _userContext,
            GetLoanStatusAction(newStatus),
            nameof(LoanApplication),
            loan.ApplicationCode,
            $"Changed loan application {loan.ApplicationCode} status from {oldStatus} to {newStatus}."));

        await dbContext.SaveChangesAsync();
    }
}

private static string GetLoanStatusAction(LoanStatus status)
{
    return status switch
    {
        LoanStatus.Confirmed => "Loan Approval",
        LoanStatus.Rejected => "Loan Rejection",
        LoanStatus.Disbursed => "Loan Disbursed",
        LoanStatus.Paid => "Loan Paid",
        _ => "Loan Status Updated"
    };
}

public async Task<List<TransactionHistoryDTO>> GetTransactionHistoryAsync(int loanApplicationId)
{
    using var dbContext = await _contextFactory.CreateDbContextAsync();
    var history = new List<TransactionHistoryDTO>();

    // 1. Process Fees
    var processFees = await dbContext.ProcessFeeDeposits
        .Where(p => p.LoanApplicationId == loanApplicationId)
        .ToListAsync();

    history.AddRange(processFees.Select(p => new TransactionHistoryDTO
    {
        TransactionDate = p.DepositDate,
        TransactionType = "Process Fee",
        Amount = p.Amount,
        Description = $"Processing Fee Deposit (Status: {p.Status})"
    }));

    // 2. Disbursements
    var disbursements = await dbContext.Disbursements
        .Where(d => d.LoanApplicationId == loanApplicationId && d.IsActive)
        .ToListAsync();

    history.AddRange(disbursements.Select(d => new TransactionHistoryDTO
    {
        TransactionDate = d.StartDate,
        TransactionType = "Disbursement",
        Amount = d.PrincipalOffered,
        Description = $"Loan Disbursement (Principal: {d.PrincipalOffered:N2})"
    }));

    // 3. Payments
    var disbursementIds = disbursements.Select(d => d.Id).ToList();
    if (disbursementIds.Any())
    {
        var payments = await dbContext.Payments
            .Include(p => p.PaymentType)
            .Where(p => disbursementIds.Contains(p.DisbursementId) && p.IsActive)
            .ToListAsync();

        history.AddRange(payments.Select(p => new TransactionHistoryDTO
        {
            TransactionDate = p.PaymentDate,
            TransactionType = "Payment",
            Amount = p.Amount,
            Description = $"Loan Payment ({p.PaymentType?.PaymentTypeName ?? "Standard"})"
        }));
    }

    return history.OrderByDescending(t => t.TransactionDate).ToList();
}

        public async Task DeleteLoanApplicationAsync(int id)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var loanApplication = await dbContext.LoanApplications.FindAsync(id);
            if (loanApplication == null)
            {
                throw new KeyNotFoundException("Loan application not found.");
            }

            // Validate status: only Applied or Rejected applications can be deleted
            if (loanApplication.Status == LoanStatus.Disbursed ||
                loanApplication.Status == LoanStatus.Confirmed ||
                loanApplication.Status == LoanStatus.Paid ||
                loanApplication.Status == LoanStatus.Rescheduled)
            {
                throw new InvalidOperationException($"Cannot delete loan application {loanApplication.ApplicationCode} because its status is {loanApplication.Status}. Only Applied or Rejected applications can be deleted.");
            }

            dbContext.LoanApplications.Remove(loanApplication);
            dbContext.ActivityLogs.Add(ActivityLogFactory.Create(
                _userContext,
                "Loan Application Deleted",
                nameof(LoanApplication),
                loanApplication.ApplicationCode,
                $"Deleted loan application {loanApplication.ApplicationCode} for amount {loanApplication.AmountRequested:N2}."
            ));

            await dbContext.SaveChangesAsync();
        }
    }
}
