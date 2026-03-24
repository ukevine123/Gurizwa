using Domain.Entities;
using Application.DTO;

namespace Application.Interfaces
{
    public interface IBorrowerType
    {
        Task<List<BorrowerType>> GetAllBorrowerTypeAsync();
        Task<BorrowerType> GetBorrowerTypeById(int id);   
        Task CreateBorrowerType(CreateBorrowTypeDTO borrowerTypeDTO);
        Task UpdateBorrowerType(int Id, UpdateBorrowTypeDTO borrowerTypeDTO);
        
    }
}
