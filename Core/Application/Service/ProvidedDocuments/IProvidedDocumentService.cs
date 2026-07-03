using Domain.Entities;
using Application.DTO;

namespace Application.Services.ProvidedDocuments
{
     public interface IProvidedDocumentService
    {
        Task<List<ProvidedDocument>> GetAllProvidedDocumentAsync();
        Task<ProvidedDocument> GetProvidedDocumentById(int id);   
        // Task CreateProvidedDocument(CreateProvidedDocumentDTO providedDocumentDTO);
        Task<ProvidedDocument> CreateProvidedDocument(CreateProvidedDocumentDTO providedDocumentDTO);
        
    }
}