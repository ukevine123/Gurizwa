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
                BorrowerName = !string.IsNullOrEmpty(d.LoanApplication.Borrower.CompanyName) ? d.LoanApplication.Borrower.CompanyName : $"{d.LoanApplication.Borrower.FirstName} {d.LoanApplication.Borrower.LastName}",
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
                var pattern = $"%{loanType.Trim()}%";
                query = query.Where(d => EF.Functions.Like(d.LoanApplication.LoanProductSetting.LoanProduct.ProductName, pattern));
            }

            var disbursements = await query.ToListAsync();

            return disbursements.Select(d => new LoanDisbursementReportDTO
            {
                LoanId = d.LoanApplicationId,
                ApplicationCode = d.LoanApplication.ApplicationCode,
                BorrowerName = !string.IsNullOrEmpty(d.LoanApplication.Borrower.CompanyName) ? d.LoanApplication.Borrower.CompanyName : $"{d.LoanApplication.Borrower.FirstName} {d.LoanApplication.Borrower.LastName}",
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
                BorrowerName = !string.IsNullOrEmpty(d.LoanApplication.Borrower.CompanyName) ? d.LoanApplication.Borrower.CompanyName : $"{d.LoanApplication.Borrower.FirstName} {d.LoanApplication.Borrower.LastName}",
                MaturityDate = d.EndDate,
                RemainingBalance = Math.Max(0, d.Amount - d.Payments.Where(p => p.IsActive).Sum(p => p.Amount)),
                Status = d.EndDate < DateTime.Now ? "Matured" : "Nearing Maturity"
            }).ToList();
        }

        public async Task<List<UserActivityReportDTO>> GetUserActivityReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var currentPersonId = await GetCurrentPersonIdAsync(dbContext);
            if (!currentPersonId.HasValue)
            {
                return new List<UserActivityReportDTO>();
            }

            var allowedUserIdsInt = await dbContext.Users
                .Where(u => u.PersonId == currentPersonId.Value)
                .Select(u => u.Id)
                .ToListAsync();
            var allowedUserIds = allowedUserIdsInt.Select(id => id.ToString()).ToList();

            var query = dbContext.ActivityLogs
                .AsNoTracking()
                .Where(a => allowedUserIds.Contains(a.UserId))
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
            var currentPersonId = await GetCurrentPersonIdAsync(dbContext);
            if (!currentPersonId.HasValue)
            {
                return new List<ActivityHeatmapDTO>();
            }

            var allowedUserIdsInt = await dbContext.Users
                .Where(u => u.PersonId == currentPersonId.Value)
                .Select(u => u.Id)
                .ToListAsync();
            var allowedUserIds = allowedUserIdsInt.Select(id => id.ToString()).ToList();

            var startDate = DateTime.UtcNow.Date.AddDays(-days);

            var logs = await dbContext.ActivityLogs
                .AsNoTracking()
                .Where(a => allowedUserIds.Contains(a.UserId) && a.Timestamp >= startDate)
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
                    BorrowerName = !string.IsNullOrEmpty(d.LoanApplication.Borrower.CompanyName) ? d.LoanApplication.Borrower.CompanyName : $"{d.LoanApplication.Borrower.FirstName} {d.LoanApplication.Borrower.LastName}",
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
                        BorrowerName = !string.IsNullOrEmpty(d.LoanApplication.Borrower.CompanyName) ? d.LoanApplication.Borrower.CompanyName : $"{d.LoanApplication.Borrower.FirstName} {d.LoanApplication.Borrower.LastName}",
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
                .Include(p => p.Account)
                .Include(p => p.Disbursement).ThenInclude(d => d.LoanApplication)
                .Where(p => p.PersonId == currentPersonId.Value &&
                            p.IsActive &&
                            p.PaymentDate >= start && p.PaymentDate < endExclusive &&
                            (p.Status == "Completed" || p.Status == "Partial"))
                .ToListAsync();

            decimal totalInterest = 0;
            decimal totalPenalty = 0;

            var interestDetails = new List<InterestIncomeDetailDTO>();
            var penaltyDetails = new List<PenaltyIncomeDetailDTO>();

            foreach (var p in payments)
            {
                if (p.PenaltyPaid > 0)
                {
                    totalPenalty += p.PenaltyPaid;
                    penaltyDetails.Add(new PenaltyIncomeDetailDTO {
                        LoanApplicationCode = p.Disbursement?.LoanApplication?.ApplicationCode ?? "N/A",
                        PaymentDate = p.PaymentDate,
                        Amount = p.PenaltyPaid,
                        Reason = "Late Payment"
                    });
                }
                
                decimal currentInterest = 0;
                if (p.InterestPaid > 0)
                {
                    currentInterest = p.InterestPaid;
                }
                else if (p.Disbursement != null && p.Disbursement.TotalInstallments > 0)
                {
                    // Fallback calculation for payments recorded before the interest fix
                    decimal totalExpectedInterest = p.Disbursement.PrincipalOffered * (p.Disbursement.InterestRate / 100);
                    decimal scheduledInterest = totalExpectedInterest / p.Disbursement.TotalInstallments;
                    currentInterest = Math.Min(p.Amount, scheduledInterest);
                }

                if (currentInterest > 0)
                {
                    totalInterest += currentInterest;
                    interestDetails.Add(new InterestIncomeDetailDTO {
                        NameOfInterest = $"Interest from {p.Disbursement?.LoanApplication?.ApplicationCode ?? "Loan"}",
                        Amount = currentInterest,
                        AccountReceiver = p.Account?.Name ?? "Main Account",
                        DateOfInterest = p.PaymentDate
                    });
                }
            }

            // 2. Processing Fees (from ProcessFeeDeposits)
            var fees = await dbContext.ProcessFeeDeposits
                .Include(f => f.Account)
                .Include(f => f.LoanApplication)
                .Where(f => (f.PersonId == currentPersonId.Value || f.LoanApplication.PersonId == currentPersonId.Value) &&
                            f.DepositDate >= start && f.DepositDate < endExclusive)
                .ToListAsync();

            var totalFees = fees.Sum(f => f.Amount);
            var processingFeeDetails = fees.Select(f => new ProcessingFeeDetailDTO {
                LoanApplicationCode = f.LoanApplication?.ApplicationCode ?? "N/A",
                PaymentDate = f.DepositDate,
                Amount = f.Amount,
                AccountReceiver = f.Account?.Name ?? "Main Account"
            }).ToList();

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
            var waiverDetails = validWaivers.Select(w => new WaiverDetailDTO {
                LoanApplicationCode = w.Disbursement?.LoanApplication?.ApplicationCode ?? "N/A",
                AmountWaived = w.Amount,
                WaiverType = w.WaiverTypeName ?? "General",
                WaivingDate = w.ApprovedDate ?? DateTime.Now
            }).ToList();

            // 4. Operating Expenses
            var expenses = await dbContext.Expenses
                .Include(e => e.Account)
                .Where(e => e.PersonId == currentPersonId.Value &&
                            e.IsActive &&
                            e.ExpenseDate >= start && e.ExpenseDate < endExclusive)
                .ToListAsync();

            var totalExpenses = expenses.Sum(e => e.Amount);
            var expenseDetails = expenses.Select(e => new OperatingExpenseDetailDTO {
                ExpenseName = e.Description ?? "Expense",
                Amount = e.Amount,
                ExpenseDate = e.ExpenseDate,
                AccountFunding = e.Account?.Name ?? "Main Account"
            }).ToList();

            return new IncomeStatementReportDTO
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalInterestIncome = totalInterest,
                TotalProcessingFeeIncome = totalFees,
                TotalPenaltyIncome = totalPenalty,
                TotalWaiversAndWriteOffs = totalWaivers,
                TotalOperatingExpenses = totalExpenses,
                InterestDetails = interestDetails,
                PenaltyDetails = penaltyDetails,
                ProcessingFeeDetails = processingFeeDetails,
                WaiverDetails = waiverDetails,
                OperatingExpenseDetails = expenseDetails
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
                int defaultedLoans = 0;
                decimal defaultedAmount = 0;

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

                            if (d.EndDate < DateTime.Now)
                            {
                                defaultedLoans++;
                                defaultedAmount += balance;
                            }
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
                    BorrowerName = !string.IsNullOrEmpty(b.CompanyName) ? b.CompanyName : $"{b.FirstName} {b.LastName}",
                    ActiveLoans = activeLoans,
                    ClosedLoans = closedLoans,
                    TotalOutstanding = outstanding,
                    TotalPaid = paid,
                    DefaultedLoans = defaultedLoans,
                    DefaultedAmount = defaultedAmount
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
                BorrowerName = !string.IsNullOrEmpty(la.Borrower.CompanyName) ? la.Borrower.CompanyName : $"{la.Borrower.FirstName} {la.Borrower.LastName}",
                Status = la.Status.ToString(),
                ApplicationDate = la.DateofApplication,
                ProcessingDays = (DateTime.Now - la.DateofApplication).Days
            }).ToList();
        }

        public async Task<List<CustomerRiskProfileReportDTO>> GetCustomerRiskProfileReportAsync() => await Task.FromResult(new List<CustomerRiskProfileReportDTO>());

        public async Task<List<LoanProductTrackerDTO>> GetLoanProductTrackerAsync()
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var currentPersonId = await GetCurrentPersonIdAsync(dbContext);
            if (!currentPersonId.HasValue) return new List<LoanProductTrackerDTO>();

            var settingsList = await dbContext.LoanProductSettings
                .Include(s => s.LoanProduct)
                .Where(s => s.PersonId == currentPersonId.Value)
                .ToListAsync();

            var disbursements = await dbContext.Disbursements
                .Include(d => d.LoanApplication).ThenInclude(la => la.Borrower)
                .Include(d => d.LoanApplication).ThenInclude(la => la.LoanProductSetting)
                .Include(d => d.Payments)
                .Where(d => d.PersonId == currentPersonId.Value || d.LoanApplication.PersonId == currentPersonId.Value)
                .ToListAsync();

            var waivers = await dbContext.Waivers
                .Include(w => w.Disbursement).ThenInclude(d => d.LoanApplication).ThenInclude(la => la.LoanProductSetting)
                .Where(w => w.Status == "Approved" && w.IsActive && (w.Disbursement.PersonId == currentPersonId.Value))
                .ToListAsync();

            var trackerReports = new List<LoanProductTrackerDTO>();

            foreach(var setting in settingsList)
            {
                var prodDisbursements = disbursements.Where(d => d.LoanApplication?.LoanProductSettingId == setting.Id).ToList();
                var prodWaivers = waivers.Where(w => w.Disbursement?.LoanApplication?.LoanProductSettingId == setting.Id).ToList();
                
                int appliedBorrowers = prodDisbursements.Select(d => d.LoanApplication.BorrowerId).Distinct().Count();
                
                decimal totalInterest = prodDisbursements.SelectMany(d => d.Payments).Where(p => p.IsActive).Sum(p => p.InterestPaid);
                
                decimal totalWaived = prodWaivers.Sum(w => w.Amount);
                
                // Estimate losses as outstanding balance of overdues/defaults
                decimal totalLosses = prodDisbursements.Where(d => d.IsActive).Sum(d => {
                    decimal paid = d.Payments.Where(p => p.IsActive).Sum(p => p.Amount);
                    // Simplified: if matured and unpaid, it's a loss, otherwise 0. For now, sum of overdue.
                    if (DateTime.Now > d.EndDate && paid < d.Amount) {
                        return d.Amount - paid;
                    }
                    return 0;
                });
                
                int totalRescheduled = prodDisbursements.Count(d => d.LoanApplication.Status == Domain.ValueObjects.LoanStatus.Rescheduled);

                var productBorrowers = prodDisbursements.Select(d => new ProductBorrowerDTO {
                    BorrowerId = d.LoanApplication.BorrowerId,
                    LoanId = d.Id,
                    BorrowerName = d.LoanApplication.Borrower != null ? (!string.IsNullOrEmpty(d.LoanApplication.Borrower.CompanyName) ? d.LoanApplication.Borrower.CompanyName : $"{d.LoanApplication.Borrower.FirstName} {d.LoanApplication.Borrower.LastName}").Trim() : "N/A",
                    ApplicationCode = d.LoanApplication.ApplicationCode ?? "N/A",
                    LoanStatus = d.LoanApplication.Status.ToString(),
                    PrincipalBalance = d.Amount - d.Payments.Where(p => p.IsActive).Sum(p => p.PrincipalPaid),
                    ApplicationDate = d.LoanApplication.DateofApplication
                }).ToList();

                trackerReports.Add(new LoanProductTrackerDTO
                {
                    ProductId = setting.LoanProductId,
                    ProductName = setting.LoanProduct?.ProductName ?? "N/A",
                    InterestRate = setting.InterestRate,
                    ProcessingFee = setting.ProcessingFee,
                    PenaltyRate = setting.PenalityRate,
                    NumberOfAppliedBorrowers = appliedBorrowers,
                    TotalInterestEarned = totalInterest,
                    TotalLosses = totalLosses,
                    TotalWaived = totalWaived,
                    TotalRescheduled = totalRescheduled,
                    Borrowers = productBorrowers
                });
            }

            return trackerReports;
        }

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
