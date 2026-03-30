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

    public LoanApplicationRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
        //    dbContext=context; 
         _contextFactory = contextFactory;
        }
        public  async Task<List<LoanApplication>> GetAllLoanApplicationsAsync()
        {
        //   List<LoanApplication> _loanApplication = dbContext.LoanApplications.ToList();
        //   return _loanApplication;
        using var dbContext = await _contextFactory.CreateDbContextAsync();
        return await dbContext.LoanApplications
            .Include(a => a.LoanProduct)
            .Include(a => a.Borrower)
            .Include(a => a.PaymentModality)
            .Include(a => a.ProvidedDocument)
            .ToListAsync();
        }
        public async Task <LoanApplication> GetLoanApplicationById(int Id)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            return  dbContext.LoanApplications.FirstOrDefault(t => t.Id == Id);
        }
         public async Task CreateLoanApplication(CreateApplicationDTO loanApplicationDTO)
        {
       using var dbContext = await _contextFactory.CreateDbContextAsync();
        var borrower = await dbContext.Borrowers.FindAsync(loanApplicationDTO.BorrowerId);
        var loanProduct = await dbContext.LoanProducts.FindAsync(loanApplicationDTO.LoanProductId);
        var providedDocument = await dbContext.ProvidedDocuments.FindAsync(loanApplicationDTO.ProvidedDocumentId);
        var paymentModality = await dbContext.PaymentModalities.FindAsync(loanApplicationDTO.PaymentModalityId);

        
            var _loanApplication = new LoanApplication
            {
                Borrower = borrower,
                LoanProduct = loanProduct,
                PaymentModality = paymentModality,
                ProvidedDocument = providedDocument,
                AmountRequested = loanApplicationDTO.AmountRequested,
                DateofApplication = loanApplicationDTO.DateofApplication, 
                Status = loanApplicationDTO.Status,
                PreferredDate = loanApplicationDTO.PreferredDate,
                ApprovedBy = loanApplicationDTO.ApprovedBy,
                CreatedBy="Admin"
              
            };
            dbContext.LoanApplications.Add(_loanApplication);
            dbContext.SaveChanges();
        }
    public async Task UpdateLoanApplication(int Id, UpdateApplicationDTO loanApplicationDTO)
{
    using var dbContext = await _contextFactory.CreateDbContextAsync();
    
    // Include LoanProduct so we can access its InterestRate for the Disbursement
    var _loanApplication = await dbContext.LoanApplications
        .Include(a => a.LoanProduct)
        .FirstOrDefaultAsync(t => t.Id == Id);

    if (_loanApplication != null)
    {
        // Update fields
        _loanApplication.PaymentModalityId = loanApplicationDTO.PaymentModalityId;
        _loanApplication.AmountRequested = loanApplicationDTO.AmountRequested;
        _loanApplication.ApprovedBy = loanApplicationDTO.ApprovedBy;
        _loanApplication.Status = loanApplicationDTO.Status; // Assuming DTO uses LoanStatus enum
        _loanApplication.PreferredDate = loanApplicationDTO.PreferredDate;
        _loanApplication.DateofApplication = loanApplicationDTO.DateofApplication;

        // FIX: Compare Enum to Enum, not Enum to String
        if (loanApplicationDTO.Status == LoanStatus.Confirmed)
        {
            // Check if disbursement already exists to avoid duplicates
            var exists = await dbContext.Disbursements.AnyAsync(d => d.LoanApplicationId == Id);
            
            if (!exists)
            {
                var disbursement = new Disbursement
                {
                    LoanApplicationId = _loanApplication.Id,
                    PaymentModalityId = _loanApplication.PaymentModalityId,
                    PrincipalOffered = _loanApplication.AmountRequested,
                    InterestRate = _loanApplication.LoanProduct?.InterestRate ?? 0,
                    Amount = _loanApplication.AmountRequested,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(12), // Adjust logic as needed
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = 1 // Use actual User ID
                };

                dbContext.Disbursements.Add(disbursement);
            }
        }

        await dbContext.SaveChangesAsync();
    }
    }
    }
    }   
    
    
        