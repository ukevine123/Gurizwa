using Application.DTO;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IRequiredDocumentService
    {
        Task<List<RequiredDocument>> GetAllRequiredDocumentsAsync();
        Task<RequiredDocument?> GetRequiredDocumentByIdAsync(int id);
        Task CreateRequiredDocumentAsync(RequiredDocumentCreateDTO requiredDocumentCreateDTO);
        // Task UpdateAccountAsync(int id, AccountUpdateDTO accountUpdateDTO);
    }
}
