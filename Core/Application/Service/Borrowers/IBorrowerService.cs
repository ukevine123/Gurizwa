using Domain.Entities;
using Application.DTO;

namespace Application.Services.Borrowers
{
    public interface IBorrowerService
    {
        Task <Borrower> GetBorrowerById(int Id);
        Task <List<Borrower>> GetAllBorrowersAsync();
        Task CreateBorrower(CreateBorrowerDTO borrowerDTO);
        Task UpdateBorrower(int Id, UpdateBorrowerDTO borrowerDTO);
    }
}