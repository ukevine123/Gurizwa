using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Application.DTO;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories

{
    public class CollateralRepository : ICollateral
    {
        // private readonly ApplicationDbContext dbContext;
        // public BorrowerRepository(ApplicationDbContext context)
     private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public CollateralRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
        //    dbContext=context; 
        _contextFactory = contextFactory;
        }
    
        public  async Task<List<Collateral>> GetAllCollateralsAsync()
        {
        using var dbContext = await _contextFactory.CreateDbContextAsync();
        return await dbContext.Collaterals
            .Include(a => a.LoanApplication)
            .ToListAsync();
        }
        public async Task <Collateral> GetCollateralByIdAsync(int Id)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            return  dbContext.Collaterals.FirstOrDefault(t => t.Id == Id);
            
        }
         public async Task CreateCollateralAsync(CollateralCreateDTO collateralDTO)
        {

            using var dbContext = await _contextFactory.CreateDbContextAsync();
        var loanApplication = await dbContext.LoanApplications.FindAsync(collateralDTO.LoanApplicationId);
            //  var borrowerType = await dbContext.BorrowerTypes.FindAsync(borrowerDTO.BorrowerTypeId);
          var _collateral = new Collateral
            {
                AssetName = collateralDTO.AssetName,
                AssetType = collateralDTO.AssetType,
                LoanApplication = loanApplication,
                Province = collateralDTO.Province,
                District = collateralDTO.District,
                Sector = collateralDTO.Sector,
                Cell = collateralDTO.Cell,
                Village = collateralDTO.Village,
                EstimatedValue = collateralDTO.EstimatedValue,
                IdentificationNumber = collateralDTO.IdentificationNumber,
                Description = collateralDTO.Description,
                ValuerName = collateralDTO.ValuerName,
                ValuationDate = collateralDTO.ValuationDate,
                
              
            };
            dbContext.Collaterals.Add(_collateral);
            dbContext.SaveChanges();
        }
        public async Task UpdateCollateralAsync(int Id, CollateralUpdateDTO collateralDTO)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
           var _collateral = await dbContext.Collaterals.FindAsync(Id);
    
          if (_collateral != null)
           {
            {
                _collateral.AssetName = collateralDTO.AssetName;
                _collateral.AssetType = collateralDTO.AssetType;
                _collateral.Province = collateralDTO.Province;
                _collateral.District = collateralDTO.District;
                _collateral.Sector = collateralDTO.Sector;
                _collateral.Cell = collateralDTO.Cell;
                _collateral.Village = collateralDTO.Village;
                _collateral.EstimatedValue = collateralDTO.EstimatedValue;
                _collateral.IdentificationNumber = collateralDTO.IdentificationNumber;
                _collateral.Description = collateralDTO.Description;
                _collateral.ValuerName = collateralDTO.ValuerName;
                _collateral.ValuationDate = collateralDTO.ValuationDate;
                dbContext.SaveChanges();
            }
        }
    }}
    }   
    
    
        