using Domain.Entities;
using Application.DTO;

namespace Application.Interfaces
{
    public interface IProvidedDocument
    {
        Task<List<ProvidedDocument>> GetAllProvidedDocumentAsync();
        Task<ProvidedDocument> GetProvidedDocumentById(int id);   
        // Task CreateProvidedDocument(CreateProvidedDocumentDTO providedDocumentDTO);
        Task<ProvidedDocument> CreateProvidedDocument(CreateProvidedDocumentDTO providedDocumentDTO);
        Task DeleteProvidedDocumentAsync(int id);
        
    }
}
