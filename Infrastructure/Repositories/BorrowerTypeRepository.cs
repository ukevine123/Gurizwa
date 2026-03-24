
using Domain.Entities;
using Infrastructure.Data;
using Application.DTO;
using Application.Interfaces;
using Application.Services.BorrowerTypes;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
public class BorrowerTypeRepository : IBorrowerType
  {
      private readonly ApplicationDbContext dbContext;

        public BorrowerTypeRepository(ApplicationDbContext context)
        {
           dbContext= context; 
        }
        // Repository for retrieving accounts data
        public async Task<List<BorrowerType>> GetAllBorrowerTypeAsync()
        {
          return await dbContext.BorrowerTypes.ToListAsync();
        }
        public async Task <BorrowerType> GetBorrowerTypeById(int Id)
        {
            return await dbContext.BorrowerTypes.FirstOrDefaultAsync(c => c.Id == Id);
        }
       public async Task CreateBorrowerType(CreateBorrowTypeDTO borrowerTypeDTO)
        {
            var borrowerType = new BorrowerType
            {
                Type = borrowerTypeDTO.Type,
                 Status = "Active",   
  
            };
            await dbContext.BorrowerTypes.AddAsync(borrowerType);
            await dbContext.SaveChangesAsync();
            
        }
         public async Task UpdateBorrowerType(int Id,UpdateBorrowTypeDTO borrowerTypeDTO)
        {
            var borrowerType = dbContext.BorrowerTypes.Find(Id);
            if (borrowerType != null)
            {
                borrowerType.Type = borrowerTypeDTO.Type;
                
            }
        }
       
}}