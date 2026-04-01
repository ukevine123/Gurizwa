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
           
    }
}