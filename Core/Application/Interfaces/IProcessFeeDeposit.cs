using Application.DTO;
using Application.Interfaces;
using Domain.Entities;


namespace Application.Interfaces
{
    public interface IProcessFeeDeposit
    {
        Task<List<ProcessFeeDeposit>> GetAllProcessFeeDepositsAsync();
        Task<ProcessFeeDeposit> GetProcessFeeDepositByIdAsync(int id);
        Task CreateProcessFeeDepositAsync(CreateProcessFeeDepositDTO createProcessFeeDepositDTO);
        Task UpdateProcessFeeDepositAsync(int id, UpdateProcessFeeDepositDTO updateProcessFeeDepositDTO);
    }
}