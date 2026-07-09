using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTO;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Service
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepository;

        public ExpenseService(IExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }

        public async Task<ExpenseDTO> GetByIdAsync(int id)
        {
            return await _expenseRepository.GetByIdAsync(id);
        }

        public async Task<List<ExpenseDTO>> GetAllAsync()
        {
            return await _expenseRepository.GetAllAsync();
        }

        public async Task<Expense> AddAsync(Expense expense)
        {
            return await _expenseRepository.AddAsync(expense);
        }

        public async Task UpdateAsync(Expense expense)
        {
            await _expenseRepository.UpdateAsync(expense);
        }

        public async Task DeleteAsync(int id)
        {
            await _expenseRepository.DeleteAsync(id);
        }
    }
}
