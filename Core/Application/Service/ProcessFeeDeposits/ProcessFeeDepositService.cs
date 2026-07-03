using Application.Interfaces;
using Domain.Entities; 
using Application.DTO;


namespace  Application.Services.ProcessFeeDeposits
{
    public class ProcessFeeDepositService : IProcessFeeDepositService
    { 
     private readonly IProcessFeeDeposit _processFeeDeposit;
        public ProcessFeeDepositService(IProcessFeeDeposit processFeeDeposit)
        {
            _processFeeDeposit = processFeeDeposit;

        }
         public async Task<List<ProcessFeeDeposit>> GetAllProcessFeeDepositsAsync()
        {
         return await _processFeeDeposit.GetAllProcessFeeDepositsAsync();
        }
         public async Task<ProcessFeeDeposit> GetProcessFeeDepositByIdAsync(int Id)
        {
            return await _processFeeDeposit.GetProcessFeeDepositByIdAsync(Id);
        }
       public async Task CreateProcessFeeDepositAsync(CreateProcessFeeDepositDTO createProcessFeeDepositDTO)
        {
            await _processFeeDeposit.CreateProcessFeeDepositAsync(createProcessFeeDepositDTO);
        }
      public async Task UpdateProcessFeeDepositAsync(int Id, UpdateProcessFeeDepositDTO updateProcessFeeDepositDTO)
        {
            await _processFeeDeposit.UpdateProcessFeeDepositAsync(Id, updateProcessFeeDepositDTO);
        }
    }
}