using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Application.DTO;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories
{
    public class ProvidedDocumentRepository : IProvidedDocument
    {

  private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        // public ProvidedDocumentRepository(ApplicationDbContext context)
         public ProvidedDocumentRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
           _contextFactory = contextFactory;
        }
        public  async Task<List<ProvidedDocument>> GetAllProvidedDocumentAsync()
        {
          List<ProvidedDocument> _providedDocument = _contextFactory.CreateDbContext().ProvidedDocuments.ToList();
          return _providedDocument;
        }
        public async Task <ProvidedDocument> GetProvidedDocumentById(int Id)
        {
            return  _contextFactory.CreateDbContext().ProvidedDocuments.FirstOrDefault(t => t.Id == Id);
        }
        public async Task<ProvidedDocument> CreateProvidedDocument(CreateProvidedDocumentDTO dto)
{
    var newDoc = new ProvidedDocument
    {
        DocumentName = dto.DocumentName,
        DocumentFile = dto.DocumentFile, 
        CreatedAt = DateTime.Now,
        CreatedBy = "Admin" 
    };

    _contextFactory.CreateDbContext().ProvidedDocuments.Add(newDoc);
    await _contextFactory.CreateDbContext().SaveChangesAsync(); // The ID is generated here

    return newDoc; // Hand the object (with its new ID) back to the Service
}
    }
    }   
    
    
        