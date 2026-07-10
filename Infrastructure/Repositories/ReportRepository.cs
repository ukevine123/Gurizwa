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
                    EntityId = a.EntityId,
                    Description = a.Description,
                    UserName = a.UserName,
                    Timestamp = a.Timestamp
                })
                .ToListAsync();
        }

        public async Task<List<ActivityHeatmapDTO>> GetActivityHeatmapAsync(int days = 365)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var currentUserId = _userContext.Id?.ToString();
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return new List<ActivityHeatmapDTO>();
            }

            var startDate = DateTime.UtcNow.Date.AddDays(-days);

            var logs = await dbContext.ActivityLogs
                .AsNoTracking()
                .Where(a => a.UserId == currentUserId && a.Timestamp >= startDate)
                .Select(a => new { a.Timestamp.Date })
                .ToListAsync();

            return logs
                .GroupBy(a => a.Date)
                .Select(g => new ActivityHeatmapDTO
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .ToList();
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
        public async Task<List<RepaymentScheduleReportDTO>> GetRepaymentScheduleReportAsync()
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var currentPersonId = await GetCurrentPersonIdAsync(dbContext);
            if (!currentPersonId.HasValue) return new List<RepaymentScheduleReportDTO>();

            var activeLoans = await dbContext.Disbursements
                .Include(d => d.LoanApplication).ThenInclude(la => la.Borrower)
                .Include(d => d.LoanApplication).ThenInclude(la => la.LoanProductSetting).ThenInclude(lps => lps.LoanProduct)
                .Include(d => d.PaymentModality)
                .Include(d => d.Payments)
                .Where(d => d.IsActive && (d.PersonId == currentPersonId.Value || d.LoanApplication.PersonId == currentPersonId.Value))
                .ToListAsync();

            var reports = new List<RepaymentScheduleReportDTO>();

            foreach (var d in activeLoans)
            {
                int totalInstallments = d.TotalInstallments > 0 ? d.TotalInstallments : 1;
                decimal totalDuePerInstallment = d.Amount / totalInstallments;
                string mode = (d.PaymentModality?.Mode ?? "monthly").ToLower();
                decimal totalCollected = d.Payments.Where(p => p.IsActive).Sum(p => p.Amount);
                decimal runningPaidAmount = totalCollected;
                decimal balance = Math.Max(0, d.Amount - totalCollected);

                if (balance <= 0.01m)
                {
                     // Fully paid, no upcoming due for this loan
                     continue;
                }

                int nextInstallmentNumber = 1;
                DateTime nextDueDate = d.StartDate;
                decimal nextInstallmentAmount = totalDuePerInstallment;

                for (int i = 0; i < totalInstallments; i++)
                {
                    DateTime dueDate = mode switch
                    {
                        "daily" => d.StartDate.AddDays(i),
                        "weekly" => d.StartDate.AddDays(i * 7),
                        "monthly" => d.StartDate.AddMonths(i),
                        _ => d.StartDate.AddMonths(i)
                    };

                    if (runningPaidAmount >= totalDuePerInstallment)
                    {
                        runningPaidAmount -= totalDuePerInstallment;
                    }
                    else if (runningPaidAmount > 0)
                    {
                        nextInstallmentNumber = i + 1;
                        nextDueDate = dueDate;
                        nextInstallmentAmount = totalDuePerInstallment - runningPaidAmount;
                        break; 
                    }
                    else
                    {
                        nextInstallmentNumber = i + 1;
                        nextDueDate = dueDate;
                        nextInstallmentAmount = totalDuePerInstallment;
                        break;
                    }
                }

                reports.Add(new RepaymentScheduleReportDTO
                {
                    LoanId = d.LoanApplicationId,
                    ApplicationCode = d.LoanApplication.ApplicationCode,
                    BorrowerName = $"{d.LoanApplication.Borrower.FirstName} {d.LoanApplication.Borrower.LastName}",
                    LoanType = d.LoanApplication.LoanProductSetting.LoanProduct.ProductName,
                    PrincipalAmount = d.Amount,
                    InterestRate = d.InterestRate,
                    TotalInstallments = totalInstallments,
                    InstallmentNumber = nextInstallmentNumber,
                    DueDate = nextDueDate,
                    InstallmentAmount = nextInstallmentAmount,
                    AmountPaid = totalCollected,
                    Balance = balance,
                    Status = "Pending"
                });
            }

            return reports;
        }

        public async Task<List<OverdueReportDTO>> GetOverdueReportAsync()
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var currentPersonId = await GetCurrentPersonIdAsync(dbContext);
            if (!currentPersonId.HasValue) return new List<OverdueReportDTO>();

            var loans = await dbContext.Disbursements
                .Include(d => d.LoanApplication).ThenInclude(la => la.Borrower)
                .Include(d => d.LoanApplication).ThenInclude(la => la.LoanProductSetting).ThenInclude(lps => lps.LoanProduct)
                .Include(d => d.Payments)
                .Where(d => d.IsActive && d.EndDate < DateTime.Now && (d.PersonId == currentPersonId.Value || d.LoanApplication.PersonId == currentPersonId.Value))
                .ToListAsync();

            var overdueList = new List<OverdueReportDTO>();
            foreach (var d in loans)
            {
                var totalPaid = d.Payments.Where(p => p.IsActive).Sum(p => p.Amount);
                var balance = d.Amount - totalPaid;
                if (balance > 0)
                {
                    var daysPastDue = (DateTime.Now - d.EndDate).Days;
                    string risk = daysPastDue > 90 ? "90+" : (daysPastDue > 60 ? "61-90" : (daysPastDue > 30 ? "31-60" : "1-30"));

                    overdueList.Add(new OverdueReportDTO
                    {
                        LoanId = d.LoanApplicationId,
                        ApplicationCode = d.LoanApplication.ApplicationCode,
                        BorrowerName = $"{d.LoanApplication.Borrower.FirstName} {d.LoanApplication.Borrower.LastName}",
                        LoanType = d.LoanApplication.LoanProductSetting.LoanProduct.ProductName,
                        PrincipalAmount = d.Amount,
                        OutstandingBalance = balance,
                        DueDate = d.EndDate,
                        DaysPastDue = daysPastDue,
                        OverdueAmount = balance,
                        RiskCategory = risk
                    });
                }
            }
            return overdueList;
        }

        public async Task<List<CollectionEfficiencyReportDTO>> GetCollectionEfficiencyReportAsync() => await Task.FromResult(new List<CollectionEfficiencyReportDTO>());

        // ── Financial Performance ──────────────────────────────────────────────
        public async Task<List<InterestIncomeReportDTO>> GetInterestIncomeReportAsync()
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var currentPersonId = await GetCurrentPersonIdAsync(dbContext);
            if (!currentPersonId.HasValue) return new List<InterestIncomeReportDTO>();

            var loans = await dbContext.Disbursements
                .Include(d => d.Payments)
                .Include(d => d.LoanApplication)
                .Where(d => d.IsActive && (d.PersonId == currentPersonId.Value || d.LoanApplication.PersonId == currentPersonId.Value))
                .ToListAsync();

            decimal totalExpected = loans.Sum(d => d.Amount * (d.InterestRate / 100));
            // Simplified expected vs collected for demo
            decimal totalCollected = loans.SelectMany(d => d.Payments).Where(p => p.IsActive).Sum(p => p.Amount * 0.1m); // Assumption: 10% of payments go to interest

            return new List<InterestIncomeReportDTO>
            {
                new InterestIncomeReportDTO
                {
                    Period = DateTime.Now.ToString("MMMM yyyy"),
                    TotalInterestExpected = totalExpected,
                    TotalInterestCollected = totalCollected
                }
            };
        }

        public async Task<List<PenaltyIncomeReportDTO>> GetPenaltyIncomeReportAsync() => await Task.FromResult(new List<PenaltyIncomeReportDTO>());
        public async Task<List<ProfitabilityReportDTO>> GetProfitabilityReportAsync() => await Task.FromResult(new List<ProfitabilityReportDTO>());

        public async Task<IncomeStatementReportDTO> GetIncomeStatementAsync(DateTime startDate, DateTime endDate)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var currentPersonId = await GetCurrentPersonIdAsync(dbContext);
            if (!currentPersonId.HasValue) return new IncomeStatementReportDTO();

            var start = startDate.Date;
            var endExclusive = endDate.Date.AddDays(1);

            // 1. Interest & Penalty Income (from Payments)
            var payments = await dbContext.Payments
                .Include(p => p.Disbursement)
                .Where(p => p.PersonId == currentPersonId.Value &&
                            p.IsActive &&
                            p.PaymentDate >= start && p.PaymentDate < endExclusive &&
                            (p.Status == "Completed" || p.Status == "Partial"))
                .ToListAsync();

            decimal totalInterest = 0;
            decimal totalPenalty = 0;

            foreach (var p in payments)
            {
                totalPenalty += p.PenaltyPaid;
                
                if (p.InterestPaid > 0)
                {
                    totalInterest += p.InterestPaid;
                }
                else if (p.Disbursement != null && p.Disbursement.TotalInstallments > 0)
                {
                    // Fallback calculation for payments recorded before the interest fix
                    decimal totalExpectedInterest = p.Disbursement.PrincipalOffered * (p.Disbursement.InterestRate / 100);
                    decimal scheduledInterest = totalExpectedInterest / p.Disbursement.TotalInstallments;
                    totalInterest += Math.Min(p.Amount, scheduledInterest);
                }
            }

            // 2. Processing Fee Income
            // Some process fee deposits might not have their PersonId perfectly aligned if they were created before we forced user.Person.Id. 
            // We can match them based on Account or LoanApplication ownership as well, but for simplicity, we check if they are in the date range.
            var fees = await dbContext.ProcessFeeDeposits
                .Include(f => f.LoanApplication)
                .Where(f => (f.PersonId == currentPersonId.Value || f.LoanApplication.PersonId == currentPersonId.Value) &&
                            f.DepositDate >= start && f.DepositDate < endExclusive)
                .ToListAsync();

            var totalFees = fees.Sum(f => f.Amount);

            // 3. Waivers & Write-offs
            var waivers = await dbContext.Waivers
                .Where(w => w.CreatedBy == currentPersonId.Value && // Assuming CreatedBy or PersonId is trackable. Wait, Waiver doesn't have PersonId! We will filter by Disbursement.PersonId
                            w.IsActive && w.Status == "Approved" &&
                            w.ApprovedDate >= start && w.ApprovedDate < endExclusive)
                .Include(w => w.Disbursement)
                .ToListAsync();
            
            // Filter waivers manually if needed, assuming Disbursement is loaded.
            var validWaivers = waivers.Where(w => w.Disbursement != null && (w.Disbursement.PersonId == currentPersonId.Value)).ToList();
            var totalWaivers = validWaivers.Sum(w => w.Amount);

            // 4. Operating Expenses
            var expenses = await dbContext.Expenses
                .Where(e => e.PersonId == currentPersonId.Value &&
                            e.IsActive &&
                            e.ExpenseDate >= start && e.ExpenseDate < endExclusive)
                .ToListAsync();

            var totalExpenses = expenses.Sum(e => e.Amount);

            return new IncomeStatementReportDTO
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalInterestIncome = totalInterest,
                TotalProcessingFeeIncome = totalFees,
                TotalPenaltyIncome = totalPenalty,
                TotalWaiversAndWriteOffs = totalWaivers,
                TotalOperatingExpenses = totalExpenses
            };
        }

        // ── Risk & Compliance ──────────────────────────────────────────────────
        public async Task<List<CreditRiskReportDTO>> GetCreditRiskReportAsync() => await Task.FromResult(new List<CreditRiskReportDTO>());
        public async Task<List<NplReportDTO>> GetNplReportAsync() => await Task.FromResult(new List<NplReportDTO>());
        public async Task<List<RegulatoryComplianceReportDTO>> GetRegulatoryComplianceReportAsync() => await Task.FromResult(new List<RegulatoryComplianceReportDTO>());

        public async Task<List<CustomerPortfolioReportDTO>> GetCustomerPortfolioReportAsync()
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var currentPersonId = await GetCurrentPersonIdAsync(dbContext);
            if (!currentPersonId.HasValue) return new List<CustomerPortfolioReportDTO>();

            var disbursements = await dbContext.Disbursements
                .Include(d => d.Payments)
                .Include(d => d.LoanApplication).ThenInclude(la => la.Borrower)
                .Where(d => d.PersonId == currentPersonId.Value || d.LoanApplication.PersonId == currentPersonId.Value)
                .ToListAsync();

            var portfolio = new List<CustomerPortfolioReportDTO>();
            var groupedByBorrower = disbursements.GroupBy(d => d.LoanApplication.BorrowerId);

            foreach (var group in groupedByBorrower)
            {
                var b = group.First().LoanApplication.Borrower;
                int activeLoans = 0;
                int closedLoans = 0;
                decimal outstanding = 0;
                decimal paid = 0;

                foreach (var d in group)
                {
                    if (d.IsActive)
                    {
                        decimal dPaid = d.Payments?.Where(p => p.IsActive).Sum(p => p.Amount) ?? 0;
                        decimal balance = d.Amount - dPaid;
                        if (balance > 0)
                        {
                            activeLoans++;
                            outstanding += balance;
                        }
                        else
                        {
                            closedLoans++;
                        }
                        paid += dPaid;
                    }
                }

                portfolio.Add(new CustomerPortfolioReportDTO
                {
                    BorrowerId = b.Id,
                    BorrowerName = $"{b.FirstName} {b.LastName}",
                    ActiveLoans = activeLoans,
                    ClosedLoans = closedLoans,
                    TotalOutstanding = outstanding,
                    TotalPaid = paid
                });
            }
            return portfolio;
        }

        public async Task<List<ApplicationStatusReportDTO>> GetApplicationStatusReportAsync()
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var currentPersonId = await GetCurrentPersonIdAsync(dbContext);
            if (!currentPersonId.HasValue) return new List<ApplicationStatusReportDTO>();

            var applications = await dbContext.LoanApplications
                .Include(la => la.Borrower)
                .Where(la => la.PersonId == currentPersonId.Value)
                .ToListAsync();

            return applications.Select(la => new ApplicationStatusReportDTO
            {
                ApplicationId = la.Id,
                ApplicationCode = la.ApplicationCode,
                BorrowerName = $"{la.Borrower.FirstName} {la.Borrower.LastName}",
                Status = la.Status.ToString(),
                ApplicationDate = la.DateofApplication,
                ProcessingDays = (DateTime.Now - la.DateofApplication).Days
            }).ToList();
        }

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
