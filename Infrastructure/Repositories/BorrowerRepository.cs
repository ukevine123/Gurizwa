using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Application.DTO;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
namespace Infrastructure.Repositories

{
    public class BorrowerRepository : IBorrower
    {
     private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
     private readonly IUserContext _userContext;    

    public BorrowerRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IUserContext userContext)
        {
        //    dbContext=context; 
        _contextFactory = contextFactory;
        _userContext = userContext;
        }
    
        public  async Task<List<Borrower>> GetAllBorrowersAsync()
        {
             if (_userContext.Id == null)
            {
                return new List<Borrower>();
            }
        using var dbContext = await _contextFactory.CreateDbContextAsync();
        
        var allowedPersonIds = await _userContext.GetAllowedPersonIdsAsync();
        if (allowedPersonIds == null || !allowedPersonIds.Any())
        {
            return new List<Borrower>();
        }

        var borrowers = await dbContext.Borrowers
            .Include(a => a.BorrowerType)
            .Where(a => allowedPersonIds.Contains(a.PersonId))
            .ToListAsync();

        var borrowerIds = borrowers.Select(b => b.Id).ToList();
        
        var disbursements = new List<Disbursement>();
        var payments = new List<Payment>();
        
        if (borrowerIds.Any())
        {
            disbursements = await dbContext.Disbursements
                .Include(d => d.LoanApplication)
                .Where(d => borrowerIds.Contains(d.LoanApplication.BorrowerId) && d.IsActive)
                .ToListAsync();

            payments = await dbContext.Payments
                .Include(p => p.Disbursement).ThenInclude(d => d.LoanApplication)
                .Where(p => borrowerIds.Contains(p.Disbursement.LoanApplication.BorrowerId) && p.IsActive)
                .ToListAsync();
        }

        foreach (var borrower in borrowers)
        {
            var bDisb = disbursements.Where(d => d.LoanApplication.BorrowerId == borrower.Id);
            var bPayments = payments.Where(p => p.Disbursement.LoanApplication.BorrowerId == borrower.Id);
            
            decimal totalAmount = bDisb.Sum(d => d.Amount);
            decimal totalPaid = bPayments.Sum(p => p.Amount);
            
            borrower.TotalDebt = Math.Max(0, totalAmount - totalPaid);
        }

        return borrowers;
        }
        public async Task <Borrower> GetBorrowerById(int Id)
        {
             
             if (_userContext.Id == null)
            {
                return null;
            }
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            
            var allowedPersonIds = await _userContext.GetAllowedPersonIdsAsync();
            return await dbContext.Borrowers
            .Where(a => allowedPersonIds.Contains(a.PersonId)) 
            .FirstOrDefaultAsync(t => t.Id == Id);
        }
         public async Task CreateBorrower(CreateBorrowerDTO borrowerDTO)
        {
            // Validate IdentificationNumber
            if (string.IsNullOrEmpty(borrowerDTO.IdentificationNumber))
            {
                throw new ArgumentException("IdentificationNumber is required.");
            }

            if (borrowerDTO.IdentificationNumber.Length > 16)
            {
                throw new ArgumentException("IdentificationNumber must not be more than 16 digits.");
            }

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

          var borrowerType = await dbContext.BorrowerTypes.FindAsync(borrowerDTO.BorrowerTypeId);
            //  var borrowerType = await dbContext.BorrowerTypes.FindAsync(borrowerDTO.BorrowerTypeId);
          var _borrower = new Borrower
            {
                FirstName = borrowerDTO.FirstName,
                LastName = borrowerDTO.LastName,
                BorrowerTypeId = borrowerDTO.BorrowerTypeId,
                PersonId = user.Person.Id,
                BorrowerType = borrowerType,
                sex = borrowerDTO.sex,
                Maritalstatus = borrowerDTO.Maritalstatus, 
                DateOfBirth = borrowerDTO.DateOfBirth ?? DateTime.Now,
                IdentificationNumber = borrowerDTO.IdentificationNumber ?? string.Empty,
                Email = borrowerDTO.Email ?? string.Empty,
                PhoneNumber = borrowerDTO.PhoneNumber ?? string.Empty,
                SpouceIdNumber = borrowerDTO.SpouceIdNumber,
                NextOfKin = borrowerDTO.NextOfKin,
                KinPhoneNumber = borrowerDTO.KinPhoneNumber,
                NextOfKinRelationship = borrowerDTO.NextOfKinRelationship,
                SpouceName = borrowerDTO.SpouceName,
                Province = borrowerDTO.Province ?? string.Empty,
                District = borrowerDTO.District ?? string.Empty,
                Sector = borrowerDTO.Sector ?? string.Empty,
                Cell = borrowerDTO.Cell ?? string.Empty,
                Village = borrowerDTO.Village ?? string.Empty,
                CompanyName = borrowerDTO.CompanyName,
                TIN = borrowerDTO.TIN,
                ContactPersonName = borrowerDTO.ContactPersonName,
                ContactPersonPhone = borrowerDTO.ContactPersonPhone,
                CreatedBy = "Admin" 
              
            };
            dbContext.Borrowers.Add(_borrower);
            dbContext.ActivityLogs.Add(ActivityLogFactory.Create(
                _userContext,
                "Borrower Created",
                nameof(Borrower),
                _borrower.IdentificationNumber,
                $"Created borrower {_borrower.FirstName ?? _borrower.CompanyName} {_borrower.LastName ?? ""} (ID: {_borrower.IdentificationNumber})."
            ));
            dbContext.SaveChanges();
        }
        public async Task UpdateBorrower(int Id,UpdateBorrowerDTO borrowerDTO)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var _borrower = await dbContext.Borrowers.FindAsync(Id);
    
            if (_borrower != null)
            {
                var oldFirstName = _borrower.FirstName ?? _borrower.CompanyName;
                var oldLastName = _borrower.LastName ?? "";

                _borrower.FirstName = borrowerDTO.FirstName;
                _borrower.LastName = borrowerDTO.LastName;
                _borrower.BorrowerTypeId = borrowerDTO.BorrowerTypeId;
                _borrower.sex = borrowerDTO.sex;
                _borrower.DateOfBirth = borrowerDTO.DateOfBirth;
                _borrower.Maritalstatus = borrowerDTO.Maritalstatus;
                _borrower.SpouceIdNumber = borrowerDTO.SpouceIdNumber;
                _borrower.SpouceName = borrowerDTO.SpouceName;
                _borrower.CompanyName = borrowerDTO.CompanyName;
                _borrower.TIN = borrowerDTO.TIN;
                _borrower.ContactPersonName = borrowerDTO.ContactPersonName;
                _borrower.ContactPersonPhone = borrowerDTO.ContactPersonPhone;

                dbContext.ActivityLogs.Add(ActivityLogFactory.Create(
                    _userContext,
                    "Borrower Edited",
                    nameof(Borrower),
                    _borrower.IdentificationNumber,
                    $"Edited borrower {_borrower.FirstName ?? _borrower.CompanyName} {_borrower.LastName ?? ""}. Name: {oldFirstName} {oldLastName} -> {_borrower.FirstName ?? _borrower.CompanyName} {_borrower.LastName ?? ""}."
                ));

                dbContext.SaveChanges();
            }
        }

        public async Task DeleteBorrowerAsync(int id)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var borrower = await dbContext.Borrowers.FindAsync(id);
            if (borrower == null)
            {
                throw new KeyNotFoundException("Borrower not found.");
            }

            // Check if borrower has any loan applications
            var hasLoans = await dbContext.LoanApplications.AnyAsync(la => la.BorrowerId == id);
            if (hasLoans)
            {
                throw new InvalidOperationException("Cannot delete borrower because they have associated loan applications.");
            }

            dbContext.Borrowers.Remove(borrower);
            dbContext.ActivityLogs.Add(ActivityLogFactory.Create(
                _userContext,
                "Borrower Deleted",
                nameof(Borrower),
                borrower.IdentificationNumber,
                $"Deleted borrower {borrower.FirstName} {borrower.LastName} (ID: {borrower.IdentificationNumber})."
            ));

            await dbContext.SaveChangesAsync();
        }

        public async Task<BorrowerDebtDTO> GetBorrowerDebtAsync(int id)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            
            var disbursements = await dbContext.Disbursements
                .Include(d => d.LoanApplication)
                .Where(d => d.LoanApplication.BorrowerId == id && d.IsActive)
                .ToListAsync();

            var payments = await dbContext.Payments
                .Include(p => p.Disbursement).ThenInclude(d => d.LoanApplication)
                .Where(p => p.Disbursement.LoanApplication.BorrowerId == id && p.IsActive)
                .ToListAsync();

            var penalties = await dbContext.Penalties
                .Include(p => p.LoanApplication)
                .Where(p => p.LoanApplication.BorrowerId == id && p.IsActive)
                .ToListAsync();

            decimal totalPrincipal = disbursements.Sum(d => d.PrincipalOffered);
            decimal currentTotalAmount = disbursements.Sum(d => d.Amount);
            decimal totalPenalties = penalties.Sum(p => p.Amount);
            
            // Total amount = Principal + Interest + Penalties
            // Therefore: Interest = Total amount - Principal - Penalties
            decimal totalInterest = currentTotalAmount - totalPrincipal - totalPenalties;
            if (totalInterest < 0) totalInterest = 0;
            
            decimal totalPaid = payments.Sum(p => p.Amount);
            decimal remainingBalance = currentTotalAmount - totalPaid;

            return new BorrowerDebtDTO
            {
                TotalPrincipal = totalPrincipal,
                TotalInterest = totalInterest,
                TotalPenalties = totalPenalties,
                TotalPaid = totalPaid,
                RemainingBalance = remainingBalance > 0 ? remainingBalance : 0
            };
        }
    }
}   
    
    
        