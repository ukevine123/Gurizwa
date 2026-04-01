using Domain.Entities;
using Application.DTO;
using Application.Interfaces;
using Application.Services.Disbursements;

namespace Application.Services.Disbursements
{
    public interface IDisbursementService
    {
        Task<List<Disbursement>> GetAllDisbursementsAsync();
        Task<Disbursement?> GetDisbursementByIdAsync(int id);
        Task CreateDisbursementAsync(CreateDisbursementDTO disbursementDTO);

        Task<List<Disbursement>> GetDisbursementsWithBalanceAsync();

    }
}

