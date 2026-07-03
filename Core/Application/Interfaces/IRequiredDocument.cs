using Application.DTO;
using Application.Interfaces;
using Domain.Entities;


namespace Application.Interfaces
{
    public interface IRequiredDocument
    {
        Task<List<RequiredDocument>> GetAllRequiredDocumentsAsync();
        Task<RequiredDocument?> GetRequiredDocumentByIdAsync(int id);
        Task CreateRequiredDocumentAsync(RequiredDocumentCreateDTO requiredDocumentCreateDTO);
        // Task UpdateAccountAsync(int id, AccountUpdateDTO accountUpdateDTO);
    }
}