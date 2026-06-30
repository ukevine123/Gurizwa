using Application.DTO;
using Application.Interfaces;
using Domain.Entities;


namespace Application.Interfaces
{
    public interface ICollateral
    {
        Task<List<Collateral>> GetAllCollateralsAsync();
        Task<Collateral?> GetCollateralByIdAsync(int id);
        Task CreateCollateralAsync(CollateralCreateDTO collateralCreateDTO);
        Task UpdateCollateralAsync(int id, CollateralUpdateDTO collateralUpdateDTO);
        Task DeleteCollateralAsync(int id);
    }
}