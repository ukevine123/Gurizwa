using System.Security.Cryptography.X509Certificates;
using Application.Interfaces;
using Domain.Entities;
using Application.DTO;


namespace Application.Services.Accounts
{
    
    public class AccountService : IAccountService
    {
        private readonly IAccount _account;

        //Constructor
        public AccountService(IAccount account)
        {
            _account = account;
        }
        
        public async Task<List<Account>> GetAllAccountsAsync()
        {
            return await _account.GetAllAccountsAsync();
        }

        public async Task<Account?> GetAccountByIdAsync(int id)
        {
            return await _account.GetAccountByIdAsync(id);
        }   

        public async Task CreateAccountAsync(AccountCreateDTO accountDTO)
        {  
            await _account.CreateAccountAsync(accountDTO);
        }

        // public async Task UpdateAccountAsync(int id, AccountUpdateDTO accountDTO)
        // {
        //     await _account.UpdateAccountAsync(id, accountDTO);
        // }
    }
}