using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Application.DTO;
using Domain.ValueObjects;
namespace Infrastructure.Repositories
{
    public class BorrowerRepository : IBorrower
    {
        private readonly ApplicationDbContext dbContext;
        public BorrowerRepository(ApplicationDbContext context)
        {
           dbContext=context; 
        }
    
        public  async Task<List<Borrower>> GetAllBorrowersAsync()
        {
          List<Borrower> _borrower = dbContext.Borrowers.ToList();
          return _borrower;
        }
        public async Task <Borrower> GetBorrowerById(int Id)
        {
            return  dbContext.Borrowers.FirstOrDefault(t => t.Id == Id);
        }
         public async Task CreateBorrower(CreateBorrowerDTO borrowerDTO)
        {
            var _borrower = new Borrower
            {
                FirstName = borrowerDTO.FirstName,
                LastName = borrowerDTO.LastName,
                BorrowerTypeId = borrowerDTO.BorrowerTypeId,
                sex = borrowerDTO.sex,
                Maritalstatus = borrowerDTO.Maritalstatus, 
                DateOfBirth = DateTime.Now,
                IdentificationNumber = borrowerDTO.IdentificationNumber,
                Email = borrowerDTO.Email,
                PhoneNumber = borrowerDTO.PhoneNumber,
                SpouceIdNumber = borrowerDTO.SpouceIdNumber,
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
            var _borrower = dbContext.Borrowers.Find(Id);
            if (_borrower != null)
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
    }
    }   
    
    
        