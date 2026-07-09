using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IUserContext _userContext;

        public ExpenseRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IUserContext userContext)
        {
            _contextFactory = contextFactory;
            _userContext = userContext;
        }

        public async Task<ExpenseDTO> GetByIdAsync(int id)
        {
            if (_userContext.PersonId == null) return new ExpenseDTO();

            using var context = await _contextFactory.CreateDbContextAsync();
            var expense = await context.Expenses
                .Include(e => e.Account)
                .FirstOrDefaultAsync(e => e.Id == id && e.PersonId == _userContext.PersonId);

            if (expense == null) return new ExpenseDTO();

            return new ExpenseDTO
            {
                Id = expense.Id,
                Amount = expense.Amount,
                ExpenseDate = expense.ExpenseDate,
                Category = expense.Category,
                Description = expense.Description,
                AccountId = expense.AccountId,
                AccountName = expense.Account?.Name ?? string.Empty,
                PersonId = expense.PersonId,
                IsActive = expense.IsActive,
                CreatedAt = expense.CreatedAt
            };
        }

        public async Task<List<ExpenseDTO>> GetAllAsync()
        {
            if (_userContext.PersonId == null) return new List<ExpenseDTO>();

            using var context = await _contextFactory.CreateDbContextAsync();
            var expenses = await context.Expenses
                .Include(e => e.Account)
                .Where(e => e.PersonId == _userContext.PersonId)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();

            return expenses.Select(expense => new ExpenseDTO
            {
                Id = expense.Id,
                Amount = expense.Amount,
                ExpenseDate = expense.ExpenseDate,
                Category = expense.Category,
                Description = expense.Description,
                AccountId = expense.AccountId,
                AccountName = expense.Account?.Name ?? string.Empty,
                PersonId = expense.PersonId,
                IsActive = expense.IsActive,
                CreatedAt = expense.CreatedAt
            }).ToList();
        }

        public async Task<Expense> AddAsync(Expense expense)
        {
            if (_userContext.Id == null) throw new Exception("User not authenticated");
            using var context = await _contextFactory.CreateDbContextAsync();
            var user = await context.Users.Include(u => u.Person).FirstOrDefaultAsync(u => u.Id == _userContext.Id);
            if (user == null || user.Person == null) throw new Exception("User or Person not found.");

            expense.PersonId = user.Person.Id;
            expense.CreatedAt = DateTime.UtcNow;
            expense.UpdatedAt = DateTime.UtcNow;
            expense.CreatedBy = _userContext.Id;

            context.Expenses.Add(expense);
            await context.SaveChangesAsync();
            return expense;
        }

        public async Task UpdateAsync(Expense expense)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var existing = await context.Expenses.FirstOrDefaultAsync(e => e.Id == expense.Id && e.PersonId == _userContext.PersonId);
            if (existing != null)
            {
                existing.Amount = expense.Amount;
                existing.ExpenseDate = expense.ExpenseDate;
                existing.Category = expense.Category;
                existing.Description = expense.Description;
                existing.AccountId = expense.AccountId;
                existing.IsActive = expense.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = _userContext.Id;

                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var existing = await context.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.PersonId == _userContext.PersonId);
            if (existing != null)
            {
                context.Expenses.Remove(existing);
                await context.SaveChangesAsync();
            }
        }
    }
}
