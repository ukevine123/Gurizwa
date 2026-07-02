using Domain.Entities;
using Application.DTO;

namespace Application.Interfaces
{
    public interface IBorrower
    {
        Task<List<Borrower>> GetAllBorrowersAsync();
        Task<Borrower> GetBorrowerById(int id);   
        Task CreateBorrower(CreateBorrowerDTO borrowerDTO);
        Task UpdateBorrower(int id, UpdateBorrowerDTO borrowerDTO);
        Task DeleteBorrowerAsync(int id);
    }
}
