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
    public class ReportRepository : IReportRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IUserContext _userContext;

        public ReportRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IUserContext userContext)
        {
            _contextFactory = contextFactory;
            _userContext = userContext;
        }

        public async Task<List<ActiveLoanSummaryDTO>> GetActiveLoansSummaryAsync()
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            
            var activeLoans = await dbContext.Disbursements
                .Include(d => d.LoanApplication)
                .ThenInclude(la => la.Borrower)
                .Include(d => d.Payments)
                .Where(d => d.IsActive)
                .ToListAsync();

            return activeLoans.Select(d => new ActiveLoanSummaryDTO
            {
                LoanId = d.LoanApplicationId,
                ApplicationCode = d.LoanApplication.ApplicationCode,
                BorrowerName = $"{d.LoanApplication.Borrower.FirstName} {d.LoanApplication.Borrower.LastName}",
                PrincipalBalance = Math.Max(0, d.Amount - d.Payments.Where(p => p.IsActive).Sum(p => p.Amount)),
                InterestRate = d.InterestRate,
                TenureMonths = d.TotalInstallments,
                StartDate = d.StartDate,
                EndDate = d.EndDate
            }).ToList();
        }

        public async Task<List<LoanDisbursementReportDTO>> GetLoanDisbursementReportAsync(DateTime startDate, DateTime endDate, string? loanType = null)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var start = startDate.Date;
            var endExclusive = endDate.Date.AddDays(1);

            var query = dbContext.Disbursements
                .Include(d => d.Account)
                .Include(d => d.LoanApplication)
                    .ThenInclude(la => la.Borrower)
                .Include(d => d.LoanApplication)
                    .ThenInclude(la => la.LoanProductSetting)
                        .ThenInclude(lps => lps.LoanProduct)
                .Where(d => d.CreatedAt >= start && d.CreatedAt < endExclusive);

            if (!string.IsNullOrWhiteSpace(loanType))
            {
                query = query.Where(d => d.LoanApplication.LoanProductSetting.LoanProduct.ProductName.Contains(loanType.Trim()));
            }

            var disbursements = await query.ToListAsync();

            return disbursements.Select(d => new LoanDisbursementReportDTO
            {
                LoanId = d.LoanApplicationId,
                ApplicationCode = d.LoanApplication.ApplicationCode,
                BorrowerName = $"{d.LoanApplication.Borrower.FirstName} {d.LoanApplication.Borrower.LastName}",
                AmountDisbursed = d.PrincipalOffered,
                LoanType = d.LoanApplication.LoanProductSetting.LoanProduct.ProductName,
                Branch = d.Account?.Provider ?? d.Account?.Name ?? "Main Branch",
                Officer = string.IsNullOrWhiteSpace(d.LoanApplication.ApprovedBy) ? "Unassigned" : d.LoanApplication.ApprovedBy,
                DisbursementDate = d.CreatedAt
            }).ToList();
        }

        public async Task<List<LoanMaturityReportDTO>> GetLoanMaturityReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();

            var query = dbContext.Disbursements
                .Include(d => d.LoanApplication)
                    .ThenInclude(la => la.Borrower)
                .Include(d => d.Payments)
                .AsQueryable();

            if (startDate.HasValue) query = query.Where(d => d.EndDate >= startDate.Value);
            if (endDate.HasValue) query = query.Where(d => d.EndDate < endDate.Value.Date.AddDays(1));

            var loans = await query.ToListAsync();

            return loans.Select(d => new LoanMaturityReportDTO
            {
                LoanId = d.LoanApplicationId,
                ApplicationCode = d.LoanApplication.ApplicationCode,
                BorrowerName = $"{d.LoanApplication.Borrower.FirstName} {d.LoanApplication.Borrower.LastName}",
                MaturityDate = d.EndDate,
                RemainingBalance = Math.Max(0, d.Amount - d.Payments.Where(p => p.IsActive).Sum(p => p.Amount)),
                Status = d.EndDate < DateTime.Now ? "Matured" : "Nearing Maturity"
            }).ToList();
        }

        public async Task<List<UserActivityReportDTO>> GetUserActivityReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();

            var query = dbContext.ActivityLogs.AsNoTracking().AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                var endExclusive = endDate.Value.Date.AddDays(1);
                query = query.Where(a => a.Timestamp < endExclusive);
            }

            return await query
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new UserActivityReportDTO
                {
                    Id = a.Id,
                    Action = a.Action,
                    EntityName = a.EntityName,
                    Description = a.Description,
                    UserName = a.UserName,
                    Timestamp = a.Timestamp
                })
                .ToListAsync();
        }

        public async Task LogActivityAsync(string action, string entityName, string entityId, string description)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();

            dbContext.ActivityLogs.Add(ActivityLogFactory.Create(
                _userContext,
                action,
                entityName,
                entityId,
                description));

            await dbContext.SaveChangesAsync();
        }
    }
}
