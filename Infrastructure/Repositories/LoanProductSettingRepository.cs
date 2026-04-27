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

    public LoanProductSettingRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
        //    dbContext=context; 
        _contextFactory = contextFactory;
        }
    
        public  async Task<List<LoanProductSetting>> GetAllLoanProductSettingsAsync()
        {
        using var dbContext = await _contextFactory.CreateDbContextAsync();
        return await dbContext.LoanProductSettings
            .Include(a => a.LoanProduct)
            .ToListAsync();
        }
        public async Task <LoanProductSetting> GetLoanProductSettingById(int Id)
        {
            // using var dbContext = await _contextFactory.CreateDbContextAsync();
            // return  dbContext.LoanProductSettings.FirstOrDefault(t => t.Id == Id);
          using var context = _contextFactory.CreateDbContext();
         return await context.LoanProductSettings
        .Include(s => s.LoanProduct)
        .FirstOrDefaultAsync(s => s.Id == Id);
        }
         public async Task CreateLoanProductSetting(CreateLoanProductSettingDTO loanProductSettingDTO)
        {

            using var dbContext = await _contextFactory.CreateDbContextAsync();
        var loanProduct = await dbContext.LoanProducts.FindAsync(loanProductSettingDTO.LoanProductId);
            //  var borrowerType = await dbContext.BorrowerTypes.FindAsync(loanProductSettingDTO.BorrowerTypeId);
          var _loanProductSetting = new LoanProductSetting
            {
                
                LoanProduct = loanProduct,
                InterestRate = loanProductSettingDTO.InterestRate,
                ProcessingFee = loanProductSettingDTO.ProcessingFee, 
                GracePeriodDays = loanProductSettingDTO.GracePeriodDays,
               
              
            };
            dbContext.LoanProductSettings.Add(_loanProductSetting);
            dbContext.SaveChanges();
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
                
                dbContext.SaveChanges();
            }
        }
    }}
    }   
    
    
        