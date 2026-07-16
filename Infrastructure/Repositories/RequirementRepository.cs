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
      private readonly IUserContext _userContext;   

      public RequirementRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory, IUserContext userContext)
        {
            _dbContextFactory = dbContextFactory;
            _userContext = userContext;
        }

      //Retrieving Accounts

      public async Task<List<Requirement>> GetAllRequirementsAsync()
        {
             await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
             if (_userContext.Id == null)
            {
                return new List<Requirement>();
            }
            var settingsPersonId = await _userContext.GetSettingsPersonIdAsync();
            return await dbContext.Requirements
            .Include(i => i.RequiredDocument)
            .Include(i => i.LoanProduct)
            .Where(i => i.PersonId == settingsPersonId) // Filter by settingsPersonId
            .ToListAsync();
        }

        public async Task<Requirement?> GetRequirementByIdAsync(int id)
        {
             using var dbContext = await _dbContextFactory.CreateDbContextAsync();
             if (_userContext.Id == null)
            {
                return null;
            }
            var settingsPersonId = await _userContext.GetSettingsPersonIdAsync();
            return await dbContext.Requirements
            .Include(i => i.RequiredDocument)
            .Include(i => i.LoanProduct)
            .Where(i => i.PersonId == settingsPersonId) // Filter by settingsPersonId
            .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task CreateRequirementAsync(RequirementCreateDTO RequirementDTO)
        {
            if (_userContext.Id == null)
            {
                throw new Exception("User not authenticated");
            }
    
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

            // 1. Get the authenticated user with their Person record
            var user = await dbContext.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Id == _userContext.Id);

            if (user == null)
            {
                throw new Exception("User record not found");
            }

            if (user.Person == null)
            {
                throw new Exception("Authenticated user does not have an associated Person record.");
            }

            // 2. Verify the RequiredDocument exists first
            var docExists = await dbContext.RequiredDocuments.AnyAsync(d => d.Id == RequirementDTO.RequiredDocumentId);
            if (!docExists) throw new Exception("Required Document not found.");

            // 3. Only check LoanProduct if it's actually provided in the DTO
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
                LoanProductId = RequirementDTO.LoanProductId,
                PersonId = user.Person.Id, // Pass the value directly
            };
            dbContext.Requirements.Add(requirement);
            await dbContext.SaveChangesAsync();
        }
    }
    
}
