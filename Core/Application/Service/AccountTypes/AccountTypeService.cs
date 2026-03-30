using System.Security.Cryptography.X509Certificates;
using Application.Interfaces;
using Domain.Entities;
using Application.DTO;


namespace Application.Services.AccountTypes
{
    
    public class AccountTypeService : IAccountTypeService
    {
        private readonly IAccountType _accountType;

        //Constructor
        public AccountTypeService(IAccountType accountType)
        {
            _accountType = accountType;
        }
        
        public async Task<List<AccountType>> GetAllAccountTypesAsync()
        {
            return await _accountType.GetAllAccountTypesAsync();
        }

        public async Task<AccountType?> GetAccountTypeByIdAsync(int id)
        {
            return await _accountType.GetAccountTypeByIdAsync(id);
        }   

        public async Task CreateAccountTypeAsync(AccountTypeCreateDTO accountTypeDTO)
        {  
            await _accountType.CreateAccountTypeAsync(accountTypeDTO);
        }

        // public async Task UpdateAccountAsync(int id, AccountUpdateDTO accountDTO)
        // {
        //     await _account.UpdateAccountAsync(id, accountDTO);
        // }
    }
}