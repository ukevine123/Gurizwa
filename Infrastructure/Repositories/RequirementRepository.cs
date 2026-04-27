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

    // 1. Verify the RequiredDocument exists first
    var docExists = await dbContext.RequiredDocuments.AnyAsync(d => d.Id == RequirementDTO.RequiredDocumentId);
    if (!docExists) throw new Exception("Required Document not found.");

    // 2. Only check LoanProduct if it's actually provided in the DTO
    if (RequirementDTO.LoanProductId.HasValue)
    {
        var lpExists = await dbContext.LoanProducts.AnyAsync(lp => lp.Id == RequirementDTO.LoanProductId.Value);
        if (!lpExists)
        {
            // Stop execution here so you don't save a NULL by mistake
            throw new Exception($"Loan Product with ID {RequirementDTO.LoanProductId} does not exist.");
        }
    }

    Requirement requirement = new()
    {
        RequiredDocumentId = RequirementDTO.RequiredDocumentId,
        LoanProductId = RequirementDTO.LoanProductId // Pass the value directly
    };
            dbContext.Requirements.Add(requirement);
            await dbContext.SaveChangesAsync();
        }
    

    
    }
    
}
