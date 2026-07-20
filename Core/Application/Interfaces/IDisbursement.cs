using Domain.Entities;
using Application.DTO;

namespace Application.Interfaces
{
    public interface IDisbursement
    {
        Task<List<Disbursement>> GetAllDisbursementsAsync();
        Task<Disbursement?> GetDisbursementByIdAsync(int id);
        Task CreateDisbursementAsync(CreateDisbursementDTO disbursementDTO); 
        Task<List<Disbursement>> GetDisbursementsWithBalanceAsync();
        Task<Disbursement?> GetDisbursementByLoanApplicationIdAsync(int loanApplicationId);

        // This method will automatically fetch the Rate and Fee from Product Settings
        Task<CreateDisbursementDTO> PrepareDisbursementFromApplicationAsync(int loanApplicationId);

        Task RescheduleLoanAsync(int oldDisbursementId, decimal newTotalAmount, int paymentModalityId, int installments, DateTime startDate, decimal interestRate);
    }
}