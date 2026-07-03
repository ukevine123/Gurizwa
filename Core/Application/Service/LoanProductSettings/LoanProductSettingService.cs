using System.Security.Cryptography.X509Certificates;
using Application.Interfaces;
using Domain.Entities;
using Application.DTO;


namespace Application.Services.LoanProductSettings
{
    
    public class LoanProductSettingService : ILoanProductSettingService
    {
        private readonly ILoanProductSetting _loanProductSetting;

        //Constructor
        public LoanProductSettingService(ILoanProductSetting loanProductSetting)
        {
            _loanProductSetting = loanProductSetting;
        }
        
        public async Task<List<LoanProductSetting>> GetAllLoanProductSettingsAsync()
        {
            return await _loanProductSetting.GetAllLoanProductSettingsAsync();
        }

        public async Task<LoanProductSetting> GetLoanProductSettingById(int id)
        {
            return await _loanProductSetting.GetLoanProductSettingById(id);
        }   

        public async Task CreateLoanProductSetting(CreateLoanProductSettingDTO loanProductSettingDTO)
        {  
            await _loanProductSetting.CreateLoanProductSetting(loanProductSettingDTO);
        }

        public async Task UpdateLoanProductSetting(int id, UpdateLoanProductSettingDTO loanProductSettingDTO)
        {
            await _loanProductSetting.UpdateLoanProductSetting(id, loanProductSettingDTO);
        }
    }
}