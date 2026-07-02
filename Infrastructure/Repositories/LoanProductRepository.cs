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
        private readonly IUserContext _userContext;

        public LoanProductRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory, IUserContext userContext)
        {
            _dbContextFactory = dbContextFactory;
            _userContext = userContext;
        }

        public async Task<List<LoanProduct>> GetAllLoanProductsAsync()
        {
            // FIX 1: Create the dbContext instance first
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            
             if (_userContext.Id == null)
            {
                return new List<LoanProduct>();
            }
            // Use ToListAsync() since the method is async
            return await dbContext.LoanProducts
             .Where(a => a.PersonId == _userContext.PersonId)
            .ToListAsync();
        }

        public async Task<LoanProduct> GetLoanProductByIdAsync(int id)
        {
            // FIX 2: Create the dbContext instance first
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
    if (_userContext.Id == null)
    {
        return null;
    }
            // FIX 3: Change 'Id' to 'id' to match the parameter name
            return await dbContext.LoanProducts
             .Where(a => a.PersonId == _userContext.PersonId) // Ensure ownership
             .FirstOrDefaultAsync(a => a.Id == id); 
        }

        public async Task<LoanProduct> CreateLoanProductAsync(LoanProductCreateDTO LoanProductDTO)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (_userContext.Id == null)
            {
                throw new Exception("User not authenticated");
            }

    // 2. Query 'Users' from the dbContext instance, NOT the factory
         var user = await dbContext.Users
        .Include(u => u.Person) // You'll likely need this to link the account
        .FirstOrDefaultAsync(u => u.Id == _userContext.Id);
            if (user == null)
            {
                throw new Exception("User record not found");
            }

            if (user.Person == null)
            {
                throw new Exception("Authenticated user does not have an associated Person record.");
            }
            LoanProduct LoanProduct = new()
            {
                ProductName = LoanProductDTO.ProductName,
                // InterestRate = LoanProductDTO.InterestRate,
                Description = LoanProductDTO.Description,
                PersonId = user.Person.Id // Associate with logged-in person
            };
            dbContext.LoanProducts.Add(LoanProduct);
            await dbContext.SaveChangesAsync();
            return LoanProduct;
        }

        public async Task UpdateLoanProductAsync(int id, LoanProductUpdateDTO loanProductUpdateDTO)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            if (_userContext.Id == null)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var LoanProduct = await dbContext.LoanProducts
                .Where(a => a.PersonId == _userContext.PersonId) // Ensure ownership
                .FirstOrDefaultAsync(a => a.Id == id);

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