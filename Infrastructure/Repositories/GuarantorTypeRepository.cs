using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Application.DTO;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class GuarantorTypeRepository : IGuarantorType
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IUserContext _userContext;  

        public GuarantorTypeRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IUserContext userContext)
        {
            _contextFactory = contextFactory;
            _userContext = userContext;
        }

        public async Task<List<GuarantorType>> GetAllGuarantorTypesAsync()
        {
            if (_userContext.Id == null)
            {
                return new List<GuarantorType>();
            }       
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.GuarantorTypes
            .Where(a => a.PersonId == _userContext.PersonId) 
            .ToListAsync();
        }

        public async Task<GuarantorType?> GetGuarantorTypeById(int id)
        {
            if (_userContext.Id == null)
            {
                return new GuarantorType();
            }
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.GuarantorTypes.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task CreateGuarantorType(CreateGuarantorTypeDTO guarantorTypeDTO)
        {
            if (_userContext.Id == null)
            {
                return;
            }    
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var user = await context.Users.Include(u => u.Person).FirstOrDefaultAsync(u => u.Id == _userContext.Id);
            if (user == null || user.Person == null) throw new Exception("User or Person not found.");

            var guarantorType = new GuarantorType
            {
                Name = guarantorTypeDTO.Name,                   
                Status = "Active",
                UpdatedBy = "System Manager",             
                PersonId = user.Person.Id
            };

            context.GuarantorTypes.Add(guarantorType);
            await context.SaveChangesAsync(); 
        }
    }
}