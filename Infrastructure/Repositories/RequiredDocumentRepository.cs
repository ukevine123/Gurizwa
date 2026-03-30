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

      public RequiredDocumentRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

      //Retrieving RequiredDocuments

      public async Task<List<RequiredDocument>> GetAllRequiredDocumentsAsync()
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            return await dbContext.RequiredDocuments.ToListAsync();
        }

        public async Task<RequiredDocument?> GetRequiredDocumentByIdAsync(int id)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            return await dbContext.RequiredDocuments.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task CreateRequiredDocumentAsync(RequiredDocumentCreateDTO RequiredDocumentDTO)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            RequiredDocument RequiredDocument = new()
            {
                DocumentName = RequiredDocumentDTO.DocumentName,
                DocumentType = RequiredDocumentDTO.DocumentType,
                
            };
            dbContext.RequiredDocuments.Add(RequiredDocument);
            await dbContext.SaveChangesAsync();
        }
    

    
    }
    
}
