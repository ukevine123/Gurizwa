using Application.DTO;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories 
{
    public class RequiredDocumentRepository : IRequiredDocument
    {
      private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
      private readonly IUserContext _userContext;

      public RequiredDocumentRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory, IUserContext userContext)
        {
            _dbContextFactory = dbContextFactory;
            _userContext = userContext;
        }

      //Retrieving RequiredDocuments

      public async Task<List<RequiredDocument>> GetAllRequiredDocumentsAsync()
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
             if (_userContext.Id == null)
            {
                return new List<RequiredDocument>();
            }
            var settingsPersonId = await _userContext.GetSettingsPersonIdAsync();
            return await dbContext.RequiredDocuments
            .Where(a => a.PersonId == settingsPersonId)
            .ToListAsync();
        }

        public async Task<RequiredDocument?> GetRequiredDocumentByIdAsync(int id)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            if (_userContext.Id == null)
            {
                return null;
            }
            var settingsPersonId = await _userContext.GetSettingsPersonIdAsync();
            return await dbContext.RequiredDocuments
            .Where(a => a.PersonId == settingsPersonId) 
            .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task CreateRequiredDocumentAsync(RequiredDocumentCreateDTO RequiredDocumentDTO)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
             if (_userContext.Id == null)
            {
                throw new Exception("User not authenticated");
            }

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

            RequiredDocument RequiredDocument = new()
            {
                DocumentName = RequiredDocumentDTO.DocumentName,
                DocumentType = RequiredDocumentDTO.DocumentType,
                 PersonId = user.Person.Id,
                
            };
            dbContext.RequiredDocuments.Add(RequiredDocument);
            await dbContext.SaveChangesAsync();
        }
    

    
    }
    
}
