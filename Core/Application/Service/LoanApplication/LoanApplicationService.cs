using Application.Interfaces;
using Domain.Entities; 
using Application.DTO;


namespace  Application.Services.LoanApplications
{
    public class LoanApplicationService : ILoanApplicationService
    { 
     private readonly ILoanApplication _loanApplication;
        public LoanApplicationService(ILoanApplication loanApplication)
        {
            _loanApplication = loanApplication;

        }
         public async Task<List<LoanApplication>> GetAllLoanApplicationsAsync()
        {
         return await _loanApplication.GetAllLoanApplicationsAsync();
        }
         public async Task<LoanApplication> GetLoanApplicationById(int Id)
        {
            return await _loanApplication.GetLoanApplicationById(Id);
        }
       public async Task CreateLoanApplication(CreateApplicationDTO loanApplicationDTO)
        {
            await _loanApplication.CreateLoanApplication( loanApplicationDTO);
        }
      public async Task UpdateLoanApplication(int Id, UpdateApplicationDTO loanApplicationDTO)
        {
            await _loanApplication.UpdateLoanApplication(Id, loanApplicationDTO);
        }
    }
}