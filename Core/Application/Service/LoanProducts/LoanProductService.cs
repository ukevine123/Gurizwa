using System.Security.Cryptography.X509Certificates;
using Application.Interfaces;
using Domain.Entities;
using Application.DTO;


namespace Application.Services.LoanProducts
{
    
    public class LoanProductService : ILoanProductService
    {
        private readonly ILoanProduct _loanProduct;

        //Constructor
        public LoanProductService(ILoanProduct loanProduct)
        {
            _loanProduct = loanProduct;
        }
        
        public async Task<List<LoanProduct>> GetAllLoanProductsAsync()
        {
            return await _loanProduct.GetAllLoanProductsAsync();
        }

        public async Task<LoanProduct> GetLoanProductByIdAsync(int id)
        {
            return await _loanProduct.GetLoanProductByIdAsync(id);
        }   

        public async Task<LoanProduct> CreateLoanProductAsync(LoanProductCreateDTO loanProductDTO)
        {  
            return await _loanProduct.CreateLoanProductAsync(loanProductDTO);
        }

        public async Task UpdateLoanProductAsync(int id, LoanProductUpdateDTO loanProductDTO)
        {
            await _loanProduct.UpdateLoanProductAsync(id, loanProductDTO);
        }
    }
}