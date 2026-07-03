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
       
     private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
     private readonly IUserContext _userContext;

    public CollateralRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IUserContext userContext)
        {
        //    dbContext=context; 
        _contextFactory = contextFactory;
        _userContext = userContext;
        }
    
        public  async Task<List<Collateral>> GetAllCollateralsAsync()
        {
        using var dbContext = await _contextFactory.CreateDbContextAsync();
        if (_userContext.Id == null)
        {
            return new List<Collateral>();
        }
        return await dbContext.Collaterals
            .Include(a => a.LoanApplication)
            .Where(a => a.PersonId == _userContext.PersonId)
            .ToListAsync();
        }
        public async Task <Collateral> GetCollateralByIdAsync(int Id)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            if (_userContext.Id == null)
            {
                return null;
            }
            return await  dbContext.Collaterals
            .Where(a => a.PersonId == _userContext.PersonId) 
            .FirstOrDefaultAsync(t => t.Id == Id);
            
        }
         public async Task CreateCollateralAsync(CollateralCreateDTO collateralDTO)
        {

         if (_userContext.Id == null)
            {
                throw new Exception("User not authenticated");
            }

            using var dbContext = await _contextFactory.CreateDbContextAsync();

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
          
            var loanApplication = await dbContext.LoanApplications.FindAsync(collateralDTO.LoanApplicationId);
            var _collateral = new Collateral
            {
                AssetName = collateralDTO.AssetName,
                AssetType = collateralDTO.AssetType,
                LoanApplication = loanApplication,
                PersonId = user.Person.Id,
                Province = collateralDTO.Province,
                District = collateralDTO.District,
                Sector = collateralDTO.Sector,
                Cell = collateralDTO.Cell,
                Village = collateralDTO.Village,
                EstimatedValue = collateralDTO.EstimatedValue,
                IdentificationNumber = collateralDTO.IdentificationNumber,
                Description = collateralDTO.Description,
                ValuerName = collateralDTO.ValuerName,
                ValuationDate = collateralDTO.ValuationDate.GetValueOrDefault(DateTime.Now),
                
              
            };
            dbContext.Collaterals.Add(_collateral);
            dbContext.ActivityLogs.Add(ActivityLogFactory.Create(
                _userContext,
                "Collateral Added",
                nameof(Collateral),
                _collateral.IdentificationNumber ?? _collateral.Id.ToString(),
                $"Added collateral {_collateral.AssetName} (Type: {_collateral.AssetType}, Value: {_collateral.EstimatedValue:N2}) for loan application {loanApplication?.ApplicationCode}."
            ));
            dbContext.SaveChanges();
        }
        public async Task UpdateCollateralAsync(int Id, CollateralUpdateDTO collateralDTO)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var _collateral = await dbContext.Collaterals
                .Include(c => c.LoanApplication)
                .FirstOrDefaultAsync(c => c.Id == Id);
    
            if (_collateral != null)
            {
                var oldAssetName = _collateral.AssetName;
                var oldEstimatedValue = _collateral.EstimatedValue;

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

                dbContext.ActivityLogs.Add(ActivityLogFactory.Create(
                    _userContext,
                    "Collateral Edited",
                    nameof(Collateral),
                    _collateral.IdentificationNumber ?? _collateral.Id.ToString(),
                    $"Edited collateral {_collateral.AssetName}. Name: {oldAssetName} -> {_collateral.AssetName}, Value: {oldEstimatedValue:N2} -> {_collateral.EstimatedValue:N2}."
                ));

                dbContext.SaveChanges();
            }
        }

        public async Task DeleteCollateralAsync(int id)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var collateral = await dbContext.Collaterals
                .Include(c => c.LoanApplication)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (collateral == null)
            {
                throw new KeyNotFoundException("Collateral not found.");
            }

            dbContext.Collaterals.Remove(collateral);
            dbContext.ActivityLogs.Add(ActivityLogFactory.Create(
                _userContext,
                "Collateral Deleted",
                nameof(Collateral),
                collateral.IdentificationNumber ?? collateral.Id.ToString(),
                $"Deleted collateral {collateral.AssetName} (Type: {collateral.AssetType}, Value: {collateral.EstimatedValue:N2}) associated with loan {collateral.LoanApplication?.ApplicationCode}."
            ));

            await dbContext.SaveChangesAsync();
        }
    }
}   
    
    
        