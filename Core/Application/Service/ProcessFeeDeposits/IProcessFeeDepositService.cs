using Domain.Entities;
using Application.DTO;

namespace Application.Services.ProcessFeeDeposits
{
     public interface IProcessFeeDepositService
    {
        Task<List<ProcessFeeDeposit>> GetAllProcessFeeDepositsAsync();
        Task<ProcessFeeDeposit> GetProcessFeeDepositByIdAsync(int id);
        Task CreateProcessFeeDepositAsync(CreateProcessFeeDepositDTO createProcessFeeDepositDTO);
        Task UpdateProcessFeeDepositAsync(int id, UpdateProcessFeeDepositDTO updateProcessFeeDepositDTO);
    }
}