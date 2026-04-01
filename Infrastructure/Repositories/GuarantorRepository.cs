using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Application.DTO;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories
{
    public class GuarantorRepository : IGuarantor
    {
        // private readonly ApplicationDbContext dbContext;
        // public GuarantorRepository(ApplicationDbContext context)
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public GuarantorRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
        //    dbContext=context; 
         _contextFactory = contextFactory;
        }
        public  async Task<List<Guarantor>> GetAllGuarantorsAsync()
        {
        //   List<Guarantor> _guarantor = dbContext.Guarantors.ToList();
        //   return _guarantor;
        using var dbContext = await _contextFactory.CreateDbContextAsync();
        return await dbContext.Guarantors
            .Include(a => a.GuarantorType)
            .Include(a => a.LoanApplication)
            .ToListAsync();
        }
        public async Task <Guarantor> GetGuarantorById(int Id)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            return  dbContext.Guarantors.FirstOrDefault(t => t.Id == Id);
        }
         public async Task CreateGuarantor(CreateGuarantorDTO guarantorDTO)
        {
              using var dbContext = await _contextFactory.CreateDbContextAsync();
        var guarantorType = await dbContext.GuarantorTypes.FindAsync(guarantorDTO.GuarantorTypeId);
        var loanApplication = await dbContext.LoanApplications.FindAsync(guarantorDTO.LoanApplicationId);
            var _guarantor = new Guarantor
            {
                GuarantorType = guarantorType,
                FirstName = guarantorDTO.FirstName,
                LastName = guarantorDTO.LastName,
                Identification = guarantorDTO.Identification,
                LoanApplication = loanApplication,
                Email = guarantorDTO.Email, 
                DateOfBirth = DateTime.Now,
                PhoneNumber = guarantorDTO.PhoneNumber,
                Province = guarantorDTO.Province,
                District = guarantorDTO.District,
                Sector = guarantorDTO.Sector,
                Cell = guarantorDTO.Cell,
                Village=guarantorDTO.Village,
                CreatedBy ="Admin"
 
            };
            dbContext.Guarantors.Add(_guarantor);
            dbContext.SaveChanges();
        }
     public async Task UpdateGuarantor(int Id,UpdateGuarantorDTO guarantorDTO)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var _guarantor = dbContext.Guarantors.Find(Id);
            if (_guarantor != null)
            {
                _guarantor.FirstName = guarantorDTO.FirstName;
                _guarantor.LastName = guarantorDTO.LastName;
                _guarantor.Identification = guarantorDTO.Identification;
                _guarantor.Email = guarantorDTO.Email;
                _guarantor.DateOfBirth = guarantorDTO.DateOfBirth;
                _guarantor.PhoneNumber = guarantorDTO.PhoneNumber;
                _guarantor.Province = guarantorDTO.Province;
                _guarantor.District = guarantorDTO.District;
                _guarantor.Sector = guarantorDTO.Sector;
                _guarantor.Cell = guarantorDTO.Cell;
                _guarantor.Village = guarantorDTO.Village;
                dbContext.SaveChanges();
            }
        }
    }
    }   
    
    
        