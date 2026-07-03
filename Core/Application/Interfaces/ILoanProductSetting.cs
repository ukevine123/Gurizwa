using Domain.Entities;
using Application.DTO;

namespace Application.Interfaces
{
    public interface ILoanProductSetting
    {
        Task<List<LoanProductSetting>> GetAllLoanProductSettingsAsync();
        Task<LoanProductSetting> GetLoanProductSettingById(int id);   
        Task CreateLoanProductSetting(CreateLoanProductSettingDTO loanProductSettingDTO);
        Task UpdateLoanProductSetting(int id, UpdateLoanProductSettingDTO loanProductSettingDTO);
    }
}