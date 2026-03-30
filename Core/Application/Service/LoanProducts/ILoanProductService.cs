using Application.DTO;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface ILoanProductService
    {
        Task<List<LoanProduct>> GetAllLoanProductsAsync();
        Task<LoanProduct?> GetLoanProductByIdAsync(int id);
        Task CreateLoanProductAsync(LoanProductCreateDTO loanProductCreateDTO);
        Task UpdateLoanProductAsync(int id, LoanProductUpdateDTO loanProductUpdateDTO);
    }
}
