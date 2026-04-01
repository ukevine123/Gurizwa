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


    }
}