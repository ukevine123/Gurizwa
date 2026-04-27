using Application.DTO;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories 
{
    public class LoanProductRepository : ILoanProduct
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public LoanProductRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<LoanProduct>> GetAllLoanProductsAsync()
        {
            // FIX 1: Create the dbContext instance first
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            
            // Use ToListAsync() since the method is async
            return await dbContext.LoanProducts.ToListAsync();
        }

        public async Task<LoanProduct> GetLoanProductByIdAsync(int id)
        {
            // FIX 2: Create the dbContext instance first
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            
            // FIX 3: Change 'Id' to 'id' to match the parameter name
            return await dbContext.LoanProducts.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<LoanProduct> CreateLoanProductAsync(LoanProductCreateDTO LoanProductDTO)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            LoanProduct LoanProduct = new()
            {
                ProductName = LoanProductDTO.ProductName,
                // InterestRate = LoanProductDTO.InterestRate,
                Description = LoanProductDTO.Description,
            };
            dbContext.LoanProducts.Add(LoanProduct);
            await dbContext.SaveChangesAsync();
            return LoanProduct;
        }

        public async Task UpdateLoanProductAsync(int id, LoanProductUpdateDTO loanProductUpdateDTO)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var LoanProduct = await dbContext.LoanProducts.FindAsync(id);

            if (LoanProduct != null)
            {
                LoanProduct.ProductName = loanProductUpdateDTO.ProductName;
                // LoanProduct.InterestRate = loanProductUpdateDTO.InterestRate;
                LoanProduct.Description = loanProductUpdateDTO.Description;

                dbContext.LoanProducts.Update(LoanProduct);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}