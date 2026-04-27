using Application.DTO;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface ILoanProductSettingService
    {
        Task<List<LoanProductSetting>> GetAllLoanProductSettingsAsync();
        Task<LoanProductSetting> GetLoanProductSettingById(int id);   
        Task CreateLoanProductSetting(CreateLoanProductSettingDTO loanProductSettingDTO);
        Task UpdateLoanProductSetting(int id, UpdateLoanProductSettingDTO loanProductSettingDTO);
    }
}