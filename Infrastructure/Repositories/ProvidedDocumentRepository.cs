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
         public async Task CreateProvidedDocument(CreateProvidedDocumentDTO providedDocumentDTO)
        {
            var _providedDocument = new ProvidedDocument
            {
                DocumentName = providedDocumentDTO.DocumentName,
                DocumentFile = providedDocumentDTO.DocumentFile, 
                CreatedBy = "Admin" 
            };
            dbContext.ProvidedDocuments.Add(_providedDocument);
            dbContext.SaveChanges();
        }
       
    }
    }   
    
    
        