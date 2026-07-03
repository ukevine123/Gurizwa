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
            var currentPersonId = await GetCurrentPersonIdAsync(dbContext);
            if (!currentPersonId.HasValue)
            {
                return new List<ActiveLoanSummaryDTO>();
            }
            
            var activeLoans = await dbContext.Disbursements
                .Include(d => d.LoanApplication)
                .ThenInclude(la => la.Borrower)
                .Include(d => d.Payments)
                .Where(d => d.IsActive &&
                    (d.PersonId == currentPersonId.Value ||
                     d.LoanApplication.PersonId == currentPersonId.Value))
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
            var currentPersonId = await GetCurrentPersonIdAsync(dbContext);
            if (!currentPersonId.HasValue)
            {
                return new List<LoanDisbursementReportDTO>();
            }

            var start = startDate.Date;
            var endExclusive = endDate.Date.AddDays(1);

            var query = dbContext.Disbursements
                .Include(d => d.Account)
                .Include(d => d.LoanApplication)
                    .ThenInclude(la => la.Borrower)
                .Include(d => d.LoanApplication)
                    .ThenInclude(la => la.LoanProductSetting)
                        .ThenInclude(lps => lps.LoanProduct)
                .Where(d => d.CreatedAt >= start &&
                    d.CreatedAt < endExclusive &&
                    (d.PersonId == currentPersonId.Value ||
                     d.LoanApplication.PersonId == currentPersonId.Value));

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
            var currentPersonId = await GetCurrentPersonIdAsync(dbContext);
            if (!currentPersonId.HasValue)
            {
                return new List<LoanMaturityReportDTO>();
            }

            var query = dbContext.Disbursements
                .Include(d => d.LoanApplication)
                    .ThenInclude(la => la.Borrower)
                .Include(d => d.Payments)
                .Where(d => d.PersonId == currentPersonId.Value ||
                    d.LoanApplication.PersonId == currentPersonId.Value)
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
            var currentUserId = _userContext.Id?.ToString();
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return new List<UserActivityReportDTO>();
            }

            var query = dbContext.ActivityLogs
                .AsNoTracking()
                .Where(a => a.UserId == currentUserId)
                .AsQueryable();

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

        // ── Repayment & Collection ─────────────────────────────────────────────
        public async Task<List<RepaymentScheduleReportDTO>> GetRepaymentScheduleReportAsync() => await Task.FromResult(new List<RepaymentScheduleReportDTO>());
        public async Task<List<OverdueReportDTO>> GetOverdueReportAsync() => await Task.FromResult(new List<OverdueReportDTO>());
        public async Task<List<CollectionEfficiencyReportDTO>> GetCollectionEfficiencyReportAsync() => await Task.FromResult(new List<CollectionEfficiencyReportDTO>());

        // ── Financial Performance ──────────────────────────────────────────────
        public async Task<List<InterestIncomeReportDTO>> GetInterestIncomeReportAsync() => await Task.FromResult(new List<InterestIncomeReportDTO>());
        public async Task<List<PenaltyIncomeReportDTO>> GetPenaltyIncomeReportAsync() => await Task.FromResult(new List<PenaltyIncomeReportDTO>());
        public async Task<List<ProfitabilityReportDTO>> GetProfitabilityReportAsync() => await Task.FromResult(new List<ProfitabilityReportDTO>());

        // ── Risk & Compliance ──────────────────────────────────────────────────
        public async Task<List<CreditRiskReportDTO>> GetCreditRiskReportAsync() => await Task.FromResult(new List<CreditRiskReportDTO>());
        public async Task<List<NplReportDTO>> GetNplReportAsync() => await Task.FromResult(new List<NplReportDTO>());
        public async Task<List<RegulatoryComplianceReportDTO>> GetRegulatoryComplianceReportAsync() => await Task.FromResult(new List<RegulatoryComplianceReportDTO>());

        // ── Customer Reports ───────────────────────────────────────────────────
        public async Task<List<CustomerPortfolioReportDTO>> GetCustomerPortfolioReportAsync() => await Task.FromResult(new List<CustomerPortfolioReportDTO>());
        public async Task<List<ApplicationStatusReportDTO>> GetApplicationStatusReportAsync() => await Task.FromResult(new List<ApplicationStatusReportDTO>());
        public async Task<List<CustomerRiskProfileReportDTO>> GetCustomerRiskProfileReportAsync() => await Task.FromResult(new List<CustomerRiskProfileReportDTO>());

        private async Task<int?> GetCurrentPersonIdAsync(ApplicationDbContext dbContext)
        {
            if (!_userContext.Id.HasValue)
            {
                return null;
            }

            return await dbContext.Users
                .Where(u => u.Id == _userContext.Id.Value)
                .Select(u => (int?)u.PersonId)
                .FirstOrDefaultAsync();
        }
    }
}
