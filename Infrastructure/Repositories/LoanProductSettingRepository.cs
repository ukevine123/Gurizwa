using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Application.DTO;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories

{
    public class LoanProductSettingRepository : ILoanProductSetting
    {
        // private readonly ApplicationDbContext dbContext;
        // public BorrowerRepository(ApplicationDbContext context)
     private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IUserContext _userContext;

    public LoanProductSettingRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IUserContext userContext)
        {
        //    dbContext=context; 
        _contextFactory = contextFactory;
        _userContext = userContext;
        }
    
        public  async Task<List<LoanProductSetting>> GetAllLoanProductSettingsAsync()
        {
        using var dbContext = await _contextFactory.CreateDbContextAsync();
        if (_userContext.Id == null)
        {
            return new List<LoanProductSetting>();
        }
        return await dbContext.LoanProductSettings
            .Include(a => a.LoanProduct)
            .Where(a => a.PersonId == _userContext.PersonId)  
            .ToListAsync();
        }
        public async Task <LoanProductSetting> GetLoanProductSettingById(int Id)
        {
            // using var dbContext = await _contextFactory.CreateDbContextAsync();
            // return  dbContext.LoanProductSettings.FirstOrDefault(t => t.Id == Id);
          using var context = _contextFactory.CreateDbContext();
        if (_userContext.Id == null)
        {
            return null;
        }
         return await context.LoanProductSettings
        .Include(s => s.LoanProduct)    
        .Where(a => a.PersonId == _userContext.PersonId)
        .FirstOrDefaultAsync(s => s.Id == Id);  
        }
         public async Task CreateLoanProductSetting(CreateLoanProductSettingDTO loanProductSettingDTO)
        {   
        if (_userContext.Id == null)
    {
        throw new UnauthorizedAccessException("User not authenticated");
    }

            using var dbContext = await _contextFactory.CreateDbContextAsync();

            var user = await dbContext.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Id == _userContext.Id);

        if (user == null)
        {
            throw new Exception("User record not found");
        }

        if (user.Person == null)
        {
            throw new Exception("Authenticated user does not have an associated Person record.");
        }
            var loanProduct = await dbContext.LoanProducts.FindAsync(loanProductSettingDTO.LoanProductId);
            
            if (loanProduct == null)
            {
                throw new Exception("Loan product not found");
            }

            var _loanProductSetting = new LoanProductSetting
            {
                LoanProductId = loanProductSettingDTO.LoanProductId,
                PersonId = user.Person.Id,
                LoanProduct = loanProduct,
                InterestRate = loanProductSettingDTO.InterestRate,
                ProcessingFee = loanProductSettingDTO.ProcessingFee, 
                GracePeriodDays = loanProductSettingDTO.GracePeriodDays,
                PenalityRate = loanProductSettingDTO.PenalityRate
            };
            
            dbContext.LoanProductSettings.Add(_loanProductSetting);
            await dbContext.SaveChangesAsync();
        }
        public async Task UpdateLoanProductSetting(int Id,UpdateLoanProductSettingDTO loanProductSettingDTO)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
           var _loanProductSetting = await dbContext.LoanProductSettings.FindAsync(Id);
    
          if (_loanProductSetting != null)
           {
            {
                _loanProductSetting.InterestRate = loanProductSettingDTO.InterestRate;
                _loanProductSetting.ProcessingFee = loanProductSettingDTO.ProcessingFee;
                _loanProductSetting.GracePeriodDays = loanProductSettingDTO.GracePeriodDays;
                _loanProductSetting.PenalityRate = loanProductSettingDTO.PenalityRate;
                await dbContext.SaveChangesAsync();
            }
        }
    }}
    }   
    
    
        