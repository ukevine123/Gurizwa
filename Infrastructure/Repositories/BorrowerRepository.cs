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
        // private readonly ApplicationDbContext dbContext;
        // public BorrowerRepository(ApplicationDbContext context)
     private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public BorrowerRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
        //    dbContext=context; 
        _contextFactory = contextFactory;
        }
    
        public  async Task<List<Borrower>> GetAllBorrowersAsync()
        {
        using var dbContext = await _contextFactory.CreateDbContextAsync();
        return await dbContext.Borrowers
            .Include(a => a.BorrowerType)
            .ToListAsync();
        }
        public async Task <Borrower> GetBorrowerById(int Id)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            return  dbContext.Borrowers.FirstOrDefault(t => t.Id == Id);
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

            using var dbContext = await _contextFactory.CreateDbContextAsync();
        var borrowerType = await dbContext.BorrowerTypes.FindAsync(borrowerDTO.BorrowerTypeId);
            //  var borrowerType = await dbContext.BorrowerTypes.FindAsync(borrowerDTO.BorrowerTypeId);
          var _borrower = new Borrower
            {
                FirstName = borrowerDTO.FirstName,
                LastName = borrowerDTO.LastName,
                BorrowerType = borrowerType,
                sex = borrowerDTO.sex,
                Maritalstatus = borrowerDTO.Maritalstatus, 
                DateOfBirth = DateTime.Now,
                IdentificationNumber = borrowerDTO.IdentificationNumber,
                Email = borrowerDTO.Email,
                PhoneNumber = borrowerDTO.PhoneNumber,
                SpouceIdNumber = borrowerDTO.SpouceIdNumber,
                NextOfKin = borrowerDTO.NextOfKin,
                KinPhoneNumber = borrowerDTO.KinPhoneNumber,
                SpouceName = borrowerDTO.SpouceName,
                Province=borrowerDTO.Province,
                District=borrowerDTO.District,
                Sector=borrowerDTO.Sector,
                Cell=borrowerDTO.Cell,
                Village=borrowerDTO.Village,
                CreatedBy = "Admin" 
              
            };
            dbContext.Borrowers.Add(_borrower);
            dbContext.SaveChanges();
        }
        public async Task UpdateBorrower(int Id,UpdateBorrowerDTO borrowerDTO)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
           var _borrower = await dbContext.Borrowers.FindAsync(Id);
    
          if (_borrower != null)
           {
            {
                _borrower.FirstName = borrowerDTO.FirstName;
                _borrower.LastName = borrowerDTO.LastName;
                _borrower.BorrowerTypeId = borrowerDTO.BorrowerTypeId;
                _borrower.sex = borrowerDTO.sex;
                _borrower.DateOfBirth = borrowerDTO.DateOfBirth;
                _borrower.Maritalstatus = borrowerDTO.Maritalstatus;
                _borrower.SpouceIdNumber = borrowerDTO.SpouceIdNumber;
                _borrower.SpouceName = borrowerDTO.SpouceName;
                dbContext.SaveChanges();
            }
        }
    }}
    }   
    
    
        