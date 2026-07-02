using Domain.Entities;
using Application.DTO;
using Application.Interfaces;

namespace Application.Services.Disbursements
{
    public class DisbursementService : IDisbursementService
    {
        private readonly IDisbursement _disbursement;

        public DisbursementService(IDisbursement disbursement)
        {
            _disbursement = disbursement;
        }

        // --- NEW RESCHEDULE METHOD ---
        public async Task RescheduleLoanAsync(int oldDisbursementId, decimal newTotalAmount, int paymentModalityId, int installments, DateTime startDate)
        {
            // Pass the parameters to the repository
            await _disbursement.RescheduleLoanAsync(oldDisbursementId, newTotalAmount, paymentModalityId, installments, startDate);
        }

        public async Task<CreateDisbursementDTO> PrepareDisbursementFromApplicationAsync(int loanApplicationId)
        {
            return await _disbursement.PrepareDisbursementFromApplicationAsync(loanApplicationId);
        }

        public async Task<Disbursement?> GetDisbursementByIdAsync(int id)
        {
            return await _disbursement.GetDisbursementByIdAsync(id);
        }

        public async Task<List<Disbursement>> GetAllDisbursementsAsync()
        {
            return await _disbursement.GetAllDisbursementsAsync();
        }

        public async Task CreateDisbursementAsync(CreateDisbursementDTO disbursementDTO)
        {
             await _disbursement.CreateDisbursementAsync(disbursementDTO);
        }

        public async Task<List<Disbursement>> GetDisbursementsWithBalanceAsync()
        {
            return await _disbursement.GetDisbursementsWithBalanceAsync();
        }

        public async Task<Disbursement?> GetDisbursementByLoanApplicationIdAsync(int loanApplicationId)
        {
            return await _disbursement.GetDisbursementByLoanApplicationIdAsync(loanApplicationId);
        }
    }
}