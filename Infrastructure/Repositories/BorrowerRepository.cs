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
        return await dbContext.Borrowers
            .Include(a => a.BorrowerType)
            .Where(a => a.PersonId == _userContext.PersonId)
            .ToListAsync();
        }
        public async Task <Borrower> GetBorrowerById(int Id)
        {
             
             if (_userContext.Id == null)
            {
                return null;
            }
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            
            return await dbContext.Borrowers
            .Where(a => a.PersonId == _userContext.PersonId) 
            .FirstOrDefaultAsync(t => t.Id == Id);
        }
         public async Task CreateBorrower(CreateBorrowerDTO borrowerDTO)
        {
            // Validate IdentificationNumber
            if (string.IsNullOrEmpty(borrowerDTO.IdentificationNumber))
            {
                throw new ArgumentException("IdentificationNumber is required.");
            }
            if (!Regex.IsMatch(borrowerDTO.IdentificationNumber, @"^\d+$"))
            {
                throw new ArgumentException("IdentificationNumber must contain only digits.");
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
                IdentificationNumber = borrowerDTO.IdentificationNumber,
                Email = borrowerDTO.Email,
                PhoneNumber = borrowerDTO.PhoneNumber,
                SpouceIdNumber = borrowerDTO.SpouceIdNumber,
                NextOfKin = borrowerDTO.NextOfKin,
                KinPhoneNumber = borrowerDTO.KinPhoneNumber,
                SpouceName = borrowerDTO.SpouceName,
                Province = borrowerDTO.Province,
                District = borrowerDTO.District,
                Sector = borrowerDTO.Sector,
                Cell = borrowerDTO.Cell,
                Village = borrowerDTO.Village,
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
    }
}   
    
    
        