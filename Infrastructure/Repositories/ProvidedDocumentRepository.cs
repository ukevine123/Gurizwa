using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Application.DTO;
using Domain.ValueObjects;
namespace Infrastructure.Repositories
{
    public class ProvidedDocumentRepository : IProvidedDocument
    {
        private readonly ApplicationDbContext dbContext;
        public ProvidedDocumentRepository(ApplicationDbContext context)
        {
           dbContext=context; 
        }
        public  async Task<List<ProvidedDocument>> GetAllProvidedDocumentAsync()
        {
          List<ProvidedDocument> _providedDocument = dbContext.ProvidedDocuments.ToList();
          return _providedDocument;
        }
        public async Task <ProvidedDocument> GetProvidedDocumentById(int Id)
        {
            return  dbContext.ProvidedDocuments.FirstOrDefault(t => t.Id == Id);
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

    dbContext.ProvidedDocuments.Add(newDoc);
    await dbContext.SaveChangesAsync(); // The ID is generated here

    return newDoc; // Hand the object (with its new ID) back to the Service
}
    }
    }   
    
    
        