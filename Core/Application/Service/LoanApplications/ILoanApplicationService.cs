using Domain.Entities;
using Application.DTO;
using Domain.ValueObjects;

namespace Application.Services.LoanApplications
{
     public interface ILoanApplicationService
    {
        Task<List<LoanApplication>> GetAllLoanApplicationsAsync();
        Task<LoanApplication?> GetLoanApplicationById(int id);   
        Task CreateLoanApplication(CreateApplicationDTO loanApplicationDTO);
        Task UpdateLoanApplication(int id, UpdateApplicationDTO loanApplicationDTO);
        Task UpdateStatusAsync(int id, LoanStatus newStatus);
        Task<List<TransactionHistoryDTO>> GetTransactionHistoryAsync(int loanApplicationId);
    }
}
