using Application.Interfaces;
using Domain.Entities; 
using Application.DTO;


namespace  Application.Services.Borrowers
{
    public class BorrowerService : IBorrowerService
    { 
     private readonly IBorrower _borrower;
        public BorrowerService(IBorrower borrower)
        {
            _borrower = borrower;

        }
         public async Task<List<Borrower>> GetAllBorrowersAsync()
        {
         return await _borrower.GetAllBorrowersAsync();
        }
         public async Task<Borrower> GetBorrowerById(int Id)
        {
            return await _borrower.GetBorrowerById(Id);
        }
       public async Task CreateBorrower(CreateBorrowerDTO borrowerDTO)
        {
            await _borrower.CreateBorrower( borrowerDTO);
        }
      public async Task UpdateBorrower(int Id, UpdateBorrowerDTO borrowerDTO)
        {
            await _borrower.UpdateBorrower(Id, borrowerDTO);
        }

        public async Task DeleteBorrowerAsync(int id)
        {
            await _borrower.DeleteBorrowerAsync(id);
        }
    }
}