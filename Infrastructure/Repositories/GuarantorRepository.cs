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
    private readonly IUserContext _userContext; 

    public GuarantorRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IUserContext userContext)
        {
        //    dbContext=context; 
         _contextFactory = contextFactory;
            _userContext = userContext;
        }
        public  async Task<List<Guarantor>> GetAllGuarantorsAsync()
        {
         if (_userContext.Id == null)
            {
                return new List<Guarantor>();
            }
        using var dbContext = await _contextFactory.CreateDbContextAsync();
        return await dbContext.Guarantors
            .Include(a => a.GuarantorType)
            .Include(a => a.LoanApplication)
            .Where(a => a.PersonId == _userContext.PersonId)
            .ToListAsync();
        }
        public async Task <Guarantor> GetGuarantorById(int Id)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
     if (_userContext.Id == null)
    {
        return null;
    }
        return await dbContext.Guarantors
       .Where(a => a.PersonId == _userContext.PersonId)
      .FirstOrDefaultAsync(t => t.Id == Id);
        }
         public async Task CreateGuarantor(CreateGuarantorDTO guarantorDTO)
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
            
              var guarantorType = await dbContext.GuarantorTypes.FindAsync(guarantorDTO.GuarantorTypeId);
              var loanApplication = await dbContext.LoanApplications.FindAsync(guarantorDTO.LoanApplicationId);
              var _guarantor = new Guarantor
            {
                GuarantorType = guarantorType,
                FirstName = guarantorDTO.FirstName,
                LastName = guarantorDTO.LastName,
                Identification = guarantorDTO.Identification,
                LoanApplication = loanApplication,
                PersonId = user.Person.Id,
                Email = guarantorDTO.Email, 
                DateOfBirth = DateTime.Now,
                PhoneNumber = guarantorDTO.PhoneNumber,
                Province = guarantorDTO.Province,
                District = guarantorDTO.District,
                Sector = guarantorDTO.Sector,
                Cell = guarantorDTO.Cell,
                Village=guarantorDTO.Village
 
            };
            dbContext.Guarantors.Add(_guarantor);
            dbContext.ActivityLogs.Add(ActivityLogFactory.Create(
                _userContext,
                "Guarantor Added",
                nameof(Guarantor),
                _guarantor.Identification,
                $"Added guarantor {_guarantor.FirstName} {_guarantor.LastName} (ID: {_guarantor.Identification}) for loan application {loanApplication?.ApplicationCode}."
            ));
            dbContext.SaveChanges();
        }
        public async Task UpdateGuarantor(int Id,UpdateGuarantorDTO guarantorDTO)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var _guarantor = await dbContext.Guarantors
                .Include(g => g.LoanApplication)
                .FirstOrDefaultAsync(g => g.Id == Id);

            if (_guarantor != null)
            {
                var oldFirstName = _guarantor.FirstName;
                var oldLastName = _guarantor.LastName;

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

                dbContext.ActivityLogs.Add(ActivityLogFactory.Create(
                    _userContext,
                    "Guarantor Edited",
                    nameof(Guarantor),
                    _guarantor.Identification,
                    $"Edited guarantor {_guarantor.FirstName} {_guarantor.LastName}. Name: {oldFirstName} {oldLastName} -> {_guarantor.FirstName} {_guarantor.LastName}."
                ));

                dbContext.SaveChanges();
            }
        }

        public async Task DeleteGuarantor(int id)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var guarantor = await dbContext.Guarantors
                .Include(g => g.LoanApplication)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (guarantor == null)
            {
                throw new KeyNotFoundException("Guarantor not found.");
            }

            dbContext.Guarantors.Remove(guarantor);
            dbContext.ActivityLogs.Add(ActivityLogFactory.Create(
                _userContext,
                "Guarantor Deleted",
                nameof(Guarantor),
                guarantor.Identification,
                $"Deleted guarantor {guarantor.FirstName} {guarantor.LastName} (ID: {guarantor.Identification}) associated with loan {guarantor.LoanApplication?.ApplicationCode}."
            ));

            await dbContext.SaveChangesAsync();
        }
    }
}   
    
    
        