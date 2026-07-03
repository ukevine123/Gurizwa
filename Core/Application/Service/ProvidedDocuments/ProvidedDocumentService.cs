using Application.Interfaces;
using Domain.Entities; 
using Application.DTO;


namespace  Application.Services.ProvidedDocuments
{
    public class ProvidedDocumentService : IProvidedDocumentService
    { 
     private readonly IProvidedDocument _providedDocument;
        public ProvidedDocumentService(IProvidedDocument providedDocument)
        {
            _providedDocument = providedDocument;

        }
         public async Task<List<ProvidedDocument>> GetAllProvidedDocumentAsync()
        {
         return await _providedDocument.GetAllProvidedDocumentAsync();
        }
         public async Task<ProvidedDocument> GetProvidedDocumentById(int Id)
        {
            return await _providedDocument.GetProvidedDocumentById(Id);
        }
       public async Task<ProvidedDocument> CreateProvidedDocument(CreateProvidedDocumentDTO providedDocumentDTO)
        {
          return await _providedDocument.CreateProvidedDocument(providedDocumentDTO);
        }
      
    }
}