using Domain.Entities;
using Application.DTO;
using Application.Interfaces;
using Application.Services.Collaterals;

namespace Application.Services.Collaterals
{
    public interface ICollateralService
    {
        Task<List<Collateral>> GetAllCollateralsAsync();
        Task<Collateral?> GetCollateralByIdAsync(int id);
        Task CreateCollateralAsync(CollateralCreateDTO collateralCreateDTO);
        Task UpdateCollateralAsync(int id, CollateralUpdateDTO collateralUpdateDTO);
        Task DeleteCollateralAsync(int id);
    }
}

