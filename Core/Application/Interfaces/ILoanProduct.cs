using Application.DTO;
using Application.Interfaces;
using Domain.Entities;


namespace Application.Interfaces
{
    public interface ILoanProduct
    {
        Task<List<LoanProduct>> GetAllLoanProductsAsync();
        Task<LoanProduct> GetLoanProductByIdAsync(int id);
        Task<LoanProduct> CreateLoanProductAsync(LoanProductCreateDTO loanProductCreateDTO);
        Task UpdateLoanProductAsync(int id, LoanProductUpdateDTO loanProductUpdateDTO);
    }
}