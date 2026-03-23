using Domain.Entities;
using Application.DTO;

namespace Application.Services.BorrowerTypes
{
    public interface IBorrowerTypeService
    {
        Task<List<BorrowerType>> GetAllBorrowerTypeAsync();
        Task<BorrowerType> GetBorrowerTypeById(int id);   
        Task CreateBorrowerType(CreateBorrowTypeDTO borrowerTypeDTO);
    }
}