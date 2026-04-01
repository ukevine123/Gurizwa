using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Application.DTO;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories
{
    public class GuarantorTypeRepository : IGuarantorType
    {
        private readonly ApplicationDbContext dbContext;
        public GuarantorTypeRepository(ApplicationDbContext context)
        {
           dbContext=context; 
        }
        // public  async Task<List<GuarantorType>> GetAllGuarantorTypesAsync()
        // {
        //   List<GuarantorType> _guarantorType = dbContext.GuarantorTypes.ToList();
        //   return _guarantorType;
        // }

        public async Task<List<GuarantorType>> GetAllGuarantorTypesAsync()
{
    // Use ToListAsync() and await it
    return await dbContext.GuarantorTypes.ToListAsync();
}
        public async Task <GuarantorType> GetGuarantorTypeById(int Id)
        {
            return  dbContext.GuarantorTypes.FirstOrDefault(t => t.Id == Id);
        }
         public async Task CreateGuarantorType(CreateGuarantorTypeDTO guarantorTypeDTO)
        {
            var _guarantorType = new GuarantorType
            {
                Name = guarantorTypeDTO.Name,
                Status = guarantorTypeDTO.Status,             
            };
            dbContext.GuarantorTypes.Add(_guarantorType);
            dbContext.SaveChanges();
        }
     
    }
    }   
    
    
        