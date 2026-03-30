using System.Security.Cryptography.X509Certificates;
using Application.Interfaces;
using Domain.Entities;
using Application.DTO;


namespace Application.Services.RequiredDocuments
{
    
    public class RequiredDocumentService : IRequiredDocumentService
    {
        private readonly IRequiredDocument _requiredDocument;

        //Constructor
        public RequiredDocumentService(IRequiredDocument requiredDocument)
        {
            _requiredDocument = requiredDocument;
        }
        public async Task<List<RequiredDocument>> GetAllRequiredDocumentsAsync()
        {
            return await _requiredDocument.GetAllRequiredDocumentsAsync();
        }
        public async Task<RequiredDocument?> GetRequiredDocumentByIdAsync(int id)
        {
            return await _requiredDocument.GetRequiredDocumentByIdAsync(id);
        }   

        public async Task CreateRequiredDocumentAsync(RequiredDocumentCreateDTO requiredDocumentDTO)
        {  
            await _requiredDocument.CreateRequiredDocumentAsync(requiredDocumentDTO);
        }

        // public async Task UpdateAccountAsync(int id, AccountUpdateDTO accountDTO)
        // {
        //     await _account.UpdateAccountAsync(id, accountDTO);
        // }
    }
}