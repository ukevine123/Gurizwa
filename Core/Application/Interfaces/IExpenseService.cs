using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTO;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IExpenseService
    {
        Task<ExpenseDTO> GetByIdAsync(int id);
        Task<List<ExpenseDTO>> GetAllAsync();
        Task<Expense> AddAsync(Expense expense);
        Task UpdateAsync(Expense expense);
        Task DeleteAsync(int id);
    }
}
