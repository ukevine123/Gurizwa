using Application.Interfaces;
using Domain.Entities; 
using Application.DTO;

namespace  Application.Services.BorrowerTypes
{
    public class BorrowerTypeService : IBorrowerTypeService
    { 
     private readonly IBorrowerType _borrowerType;
        public BorrowerTypeService(IBorrowerType borrowerType)
        {
            _borrowerType = borrowerType;

        }
         public async Task<List<BorrowerType>> GetAllBorrowerTypeAsync()
        {
         return await _borrowerType.GetAllBorrowerTypeAsync();
        }
         public async Task<BorrowerType> GetBorrowerTypeById(int Id)
        {
            return await _borrowerType.GetBorrowerTypeById(Id);
        }
       public async Task CreateBorrowerType(CreateBorrowTypeDTO borrowerTypeDTO)
        {
            await _borrowerType.CreateBorrowerType( borrowerTypeDTO);
        }
    //   public async Task UpdateBorrower(int Id, UpdateBorrowerDTO borrowerDTO)
    //     {
    //         await _borrowerType.UpdateBorrower(Id, borrowerDTO);
    //     }
    }

    
}