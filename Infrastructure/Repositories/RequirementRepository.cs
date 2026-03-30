using Application.DTO;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories 
{
    public class RequirementRepository : IRequirement
    {
      private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

      public RequirementRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

      //Retrieving Accounts

      public async Task<List<Requirement>> GetAllRequirementsAsync()
        {
             await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            return await dbContext.Requirements
            .Include(i => i.RequiredDocument)
            .Include(i => i.LoanProduct)
            .ToListAsync();
        }

        public async Task<Requirement?> GetRequirementByIdAsync(int id)
        {
             using var dbContext = await _dbContextFactory.CreateDbContextAsync();
              return await dbContext.Requirements
            .Include(i => i.RequiredDocument)
            .Include(i => i.LoanProduct)
            .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task CreateRequirementAsync(RequirementCreateDTO RequirementDTO)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            Requirement Requirement = new()
            {
                RequiredDocumentId = RequirementDTO.RequiredDocumentId,
                LoanProductId = RequirementDTO.LoanProductId,
                
            };
            dbContext.Requirements.Add(Requirement);
            await dbContext.SaveChangesAsync();
        }
    

    
    }
    
}
