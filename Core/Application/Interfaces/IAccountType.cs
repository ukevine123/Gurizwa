using Application.DTO;
using Application.Interfaces;
using Domain.Entities;


namespace Application.Interfaces
{
    public interface IAccountType
    {
        Task<List<AccountType>> GetAllAccountTypesAsync();
        Task<AccountType?> GetAccountTypeByIdAsync(int id);
        Task CreateAccountTypeAsync(AccountTypeCreateDTO accountTypeCreateDTO);
        // Task UpdateAccountAsync(int id, AccountUpdateDTO accountUpdateDTO);
    }
}