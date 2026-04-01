using Domain.Entities;
using Application.DTO;
using Application.Interfaces;

namespace Application.Services.Collaterals
{
    public class CollateralService : ICollateralService
    {
        private readonly ICollateral _collateral;

        public CollateralService(ICollateral collateral)
        {
            _collateral = collateral;
        }

         public async Task<Collateral?> GetCollateralByIdAsync(int id)
        {
            return await _collateral.GetCollateralByIdAsync(id);
    
        }
        public async Task<List<Collateral>> GetAllCollateralsAsync()
        {
            return await _collateral.GetAllCollateralsAsync();
        }

         public async Task CreateCollateralAsync(CollateralCreateDTO collateralDTO)
        {
             await _collateral.CreateCollateralAsync(collateralDTO);
        }

        public async Task UpdateCollateralAsync(int id, CollateralUpdateDTO collateralUpdateDTO)
        {
            await _collateral.UpdateCollateralAsync(id, collateralUpdateDTO);
        }
           
    }
}