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

        // ── Repayment & Collection ──────────────────────────────────────────
        public async Task<List<RepaymentScheduleReportDTO>> GetRepaymentScheduleReportAsync()
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var disbursements = await dbContext.Disbursements
                .Include(d => d.LoanApplication)
                    .ThenInclude(la => la.Borrower)
                .Include(d => d.LoanApplication)
                    .ThenInclude(la => la.LoanProductSetting)
                        .ThenInclude(lps => lps.LoanProduct)
                .Include(d => d.PaymentModality)
                .Include(d => d.Payments)
                .Where(d => d.IsActive)
                .ToListAsync();

            var results = new List<RepaymentScheduleReportDTO>();

            foreach (var d in disbursements)
            {
                if (d.TotalInstallments <= 0) continue;

                decimal installmentAmount = d.Amount / d.TotalInstallments;
                string mode = (d.PaymentModality?.Mode ?? "monthly").ToLower();
                decimal totalCollected = d.Payments?.Where(p => p.IsActive).Sum(p => p.Amount) ?? 0;
                decimal runningPaidAmount = totalCollected;

                for (int i = 0; i < d.TotalInstallments; i++)
                {
                    DateTime dueDate = mode switch
                    {
                        "daily" => d.StartDate.AddDays(i),
                        "weekly" => d.StartDate.AddDays(i * 7),
                        "monthly" => d.StartDate.AddMonths(i),
                        _ => d.StartDate.AddMonths(i)
                    };

                    string currentStatus = "Pending";
                    decimal amountPaidForThis = 0;

                    if (runningPaidAmount >= installmentAmount)
                    {
                        currentStatus = "Paid";
                        amountPaidForThis = installmentAmount;
                        runningPaidAmount -= installmentAmount;
                    }
                    else if (runningPaidAmount > 0)
                    {
                        currentStatus = "Partial";
                        amountPaidForThis = runningPaidAmount;
                        runningPaidAmount = 0;
                    }
                    else
                    {
                        if (DateTime.Today > dueDate.Date) currentStatus = "Overdue";
                    }

                    results.Add(new RepaymentScheduleReportDTO
                    {
                        LoanId = d.LoanApplicationId,
                        ApplicationCode = d.LoanApplication.ApplicationCode,
                        BorrowerName = $"{d.LoanApplication.Borrower.FirstName} {d.LoanApplication.Borrower.LastName}",
                        LoanType = d.LoanApplication.LoanProductSetting.LoanProduct.ProductName,
                        PrincipalAmount = d.PrincipalOffered,
                        InterestRate = d.InterestRate,
                        TotalInstallments = d.TotalInstallments,
                        InstallmentNumber = i + 1,
                        DueDate = dueDate,
                        InstallmentAmount = installmentAmount,
                        AmountPaid = amountPaidForThis,
                        Balance = Math.Max(0, d.Amount - totalCollected),
                        Status = currentStatus
                    });
                }
            }

            return results.OrderBy(r => r.DueDate).ToList();
        }

        public async Task<List<OverdueReportDTO>> GetOverdueReportAsync()
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var disbursements = await dbContext.Disbursements
                .Include(d => d.LoanApplication)
                    .ThenInclude(la => la.Borrower)
                .Include(d => d.LoanApplication)
                    .ThenInclude(la => la.LoanProductSetting)
                        .ThenInclude(lps => lps.LoanProduct)
                .Include(d => d.PaymentModality)
                .Include(d => d.Payments)
                .Where(d => d.IsActive)
                .ToListAsync();

            var results = new List<OverdueReportDTO>();

            foreach (var d in disbursements)
            {
                if (d.TotalInstallments <= 0) continue;

                decimal installmentAmount = d.Amount / d.TotalInstallments;
                string mode = (d.PaymentModality?.Mode ?? "monthly").ToLower();
                decimal totalCollected = d.Payments?.Where(p => p.IsActive).Sum(p => p.Amount) ?? 0;
                decimal runningPaidAmount = totalCollected;

                DateTime? earliestMissedDueDate = null;
                decimal overdueAmount = 0;

                for (int i = 0; i < d.TotalInstallments; i++)
                {
                    DateTime dueDate = mode switch
                    {
                        "daily" => d.StartDate.AddDays(i),
                        "weekly" => d.StartDate.AddDays(i * 7),
                        "monthly" => d.StartDate.AddMonths(i),
                        _ => d.StartDate.AddMonths(i)
                    };

                    decimal unpaidForThis = 0;
                    if (runningPaidAmount >= installmentAmount)
                    {
                        runningPaidAmount -= installmentAmount;
                    }
                    else if (runningPaidAmount > 0)
                    {
                        unpaidForThis = installmentAmount - runningPaidAmount;
                        runningPaidAmount = 0;
                    }
                    else
                    {
                        unpaidForThis = installmentAmount;
                    }

                    if (unpaidForThis > 0 && DateTime.Today > dueDate.Date)
                    {
                        overdueAmount += unpaidForThis;
                        if (earliestMissedDueDate == null)
                        {
                            earliestMissedDueDate = dueDate;
                        }
                    }
                }

                if (overdueAmount > 0.01m && earliestMissedDueDate.HasValue)
                {
                    int daysPastDue = (DateTime.Today - earliestMissedDueDate.Value.Date).Days;
                    string riskCategory = daysPastDue switch
                    {
                        <= 30 => "1-30 Days",
                        <= 60 => "31-60 Days",
                        <= 90 => "61-90 Days",
                        _ => "90+ Days (Default)"
                    };

                    results.Add(new OverdueReportDTO
                    {
                        LoanId = d.LoanApplicationId,
                        ApplicationCode = d.LoanApplication.ApplicationCode,
                        BorrowerName = $"{d.LoanApplication.Borrower.FirstName} {d.LoanApplication.Borrower.LastName}",
                        LoanType = d.LoanApplication.LoanProductSetting.LoanProduct.ProductName,
                        PrincipalAmount = d.PrincipalOffered,
                        OutstandingBalance = Math.Max(0, d.Amount - totalCollected),
                        DueDate = earliestMissedDueDate.Value,
                        DaysPastDue = daysPastDue,
                        OverdueAmount = overdueAmount,
                        RiskCategory = riskCategory
                    });
                }
            }

            return results.OrderByDescending(r => r.DaysPastDue).ToList();
        }

        public async Task<List<CollectionEfficiencyReportDTO>> GetCollectionEfficiencyReportAsync()
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var disbursements = await dbContext.Disbursements
                .Include(d => d.LoanApplication)
                    .ThenInclude(la => la.Borrower)
                .Include(d => d.LoanApplication)
                    .ThenInclude(la => la.LoanProductSetting)
                        .ThenInclude(lps => lps.LoanProduct)
                .Include(d => d.PaymentModality)
                .Include(d => d.Payments)
                .Where(d => d.IsActive)
                .ToListAsync();

            var results = new List<CollectionEfficiencyReportDTO>();

            foreach (var d in disbursements)
            {
                if (d.TotalInstallments <= 0) continue;

                decimal installmentAmount = d.Amount / d.TotalInstallments;
                string mode = (d.PaymentModality?.Mode ?? "monthly").ToLower();
                decimal totalCollected = d.Payments?.Where(p => p.IsActive).Sum(p => p.Amount) ?? 0;
                
                decimal totalDueSoFar = 0;

                for (int i = 0; i < d.TotalInstallments; i++)
                {
                    DateTime dueDate = mode switch
                    {
                        "daily" => d.StartDate.AddDays(i),
                        "weekly" => d.StartDate.AddDays(i * 7),
                        "monthly" => d.StartDate.AddMonths(i),
                        _ => d.StartDate.AddMonths(i)
                    };

                    if (DateTime.Today >= dueDate.Date)
                    {
                        totalDueSoFar += installmentAmount;
                    }
                }

                if (totalDueSoFar <= 0)
                {
                    totalDueSoFar = installmentAmount; // default to first installment due
                }

                decimal collectionRate = totalDueSoFar > 0 ? (totalCollected / totalDueSoFar) * 100 : 0;
                if (collectionRate > 100) collectionRate = 100;

                string category = collectionRate switch
                {
                    >= 95 => "Excellent",
                    >= 85 => "Good",
                    >= 70 => "Fair",
                    _ => "Poor"
                };

                results.Add(new CollectionEfficiencyReportDTO
                {
                    LoanId = d.LoanApplicationId,
                    ApplicationCode = d.LoanApplication.ApplicationCode,
                    BorrowerName = $"{d.LoanApplication.Borrower.FirstName} {d.LoanApplication.Borrower.LastName}",
                    LoanType = d.LoanApplication.LoanProductSetting.LoanProduct.ProductName,
                    TotalDue = totalDueSoFar,
                    TotalCollected = totalCollected,
                    OutstandingBalance = Math.Max(0, d.Amount - totalCollected),
                    CollectionRate = collectionRate,
                    EfficiencyCategory = category
                });
            }

            return results.OrderBy(r => r.CollectionRate).ToList();
        }

        // ── Financial Performance ───────────────────────────────────────────
        public async Task<List<InterestIncomeReportDTO>> GetInterestIncomeReportAsync(DateTime startDate, DateTime endDate)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var start = startDate.Date;
            var endExclusive = endDate.Date.AddDays(1);

            var disbursements = await dbContext.Disbursements
                .Include(d => d.LoanApplication)
                    .ThenInclude(la => la.Borrower)
                .Include(d => d.LoanApplication)
                    .ThenInclude(la => la.LoanProductSetting)
                        .ThenInclude(lps => lps.LoanProduct)
                .Include(d => d.Payments)
                .Where(d => d.CreatedAt >= start && d.CreatedAt < endExclusive)
                .ToListAsync();

            return disbursements.Select(d => {
                decimal interestExpected = d.Amount - d.PrincipalOffered;
                decimal interestCollected = d.Payments?.Where(p => p.IsActive).Sum(p => p.InterestPaid) ?? 0;
                return new InterestIncomeReportDTO
                {
                    LoanId = d.LoanApplicationId,
                    ApplicationCode = d.LoanApplication.ApplicationCode,
                    BorrowerName = $"{d.LoanApplication.Borrower.FirstName} {d.LoanApplication.Borrower.LastName}",
                    LoanType = d.LoanApplication.LoanProductSetting.LoanProduct.ProductName,
                    InterestRate = d.InterestRate,
                    TotalInterestExpected = interestExpected,
                    InterestCollected = interestCollected
                };
            }).ToList();
        }

        public async Task<List<PenaltyChargesReportDTO>> GetPenaltyChargesReportAsync(DateTime startDate, DateTime endDate)
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var start = startDate.Date;
            var endExclusive = endDate.Date.AddDays(1);

            var penalties = await dbContext.Penalties
                .Include(p => p.LoanApplication)
                    .ThenInclude(la => la.Borrower)
                .Where(p => p.CreatedAt >= start && p.CreatedAt < endExclusive && p.IsActive)
                .ToListAsync();

            var disbursements = await dbContext.Disbursements
                .Include(d => d.Payments)
                .Where(d => d.IsActive)
                .ToListAsync();

            var results = new List<PenaltyChargesReportDTO>();

            foreach (var p in penalties)
            {
                var disb = disbursements.FirstOrDefault(d => d.LoanApplicationId == p.LoanApplicationId);
                decimal penaltyPaid = 0;
                if (disb != null)
                {
                    penaltyPaid = disb.Payments?.Where(pay => pay.IsActive).Sum(pay => pay.PenaltyPaid) ?? 0;
                }

                results.Add(new PenaltyChargesReportDTO
                {
                    LoanId = p.LoanApplicationId,
                    ApplicationCode = p.LoanApplication?.ApplicationCode ?? "N/A",
                    BorrowerName = p.LoanApplication != null ? $"{p.LoanApplication.Borrower.FirstName} {p.LoanApplication.Borrower.LastName}" : "Unknown",
                    PenaltyAmountLevied = p.Amount,
                    PenaltyPaid = penaltyPaid,
                    PenaltyDate = p.CreatedAt,
                    Description = p.Description
                });
            }

            return results.OrderByDescending(r => r.PenaltyDate).ToList();
        }

        public async Task<List<ProfitabilityReportDTO>> GetProfitabilityReportAsync()
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();

            var disbursements = await dbContext.Disbursements
                .Include(d => d.LoanApplication)
                    .ThenInclude(la => la.Borrower)
                .Include(d => d.LoanApplication)
                    .ThenInclude(la => la.LoanProductSetting)
                        .ThenInclude(lps => lps.LoanProduct)
                .Include(d => d.PaymentModality)
                .Include(d => d.Payments)
                .ToListAsync();

            var grouped = disbursements
                .GroupBy(d => d.LoanApplication.LoanProductSetting.LoanProduct.ProductName);

            var results = new List<ProfitabilityReportDTO>();

            foreach (var g in grouped)
            {
                var loanType = g.Key;
                var totalLoans = g.Count();
                var totalDisbursed = g.Sum(d => d.PrincipalOffered);
                var interestCollected = g.Sum(d => d.Payments?.Where(p => p.IsActive).Sum(p => p.InterestPaid) ?? 0);
                var penaltyCollected = g.Sum(d => d.Payments?.Where(p => p.IsActive).Sum(p => p.PenaltyPaid) ?? 0);
                var feesCollected = g.Sum(d => d.LoanApplication.LoanProductSetting?.ProcessingFee ?? 0);

                decimal writeOff = 0;
                foreach (var d in g)
                {
                    if (d.TotalInstallments <= 0) continue;
                    decimal installmentAmount = d.Amount / d.TotalInstallments;
                    string mode = (d.PaymentModality?.Mode ?? "monthly").ToLower();
                    decimal totalCollected = d.Payments?.Where(p => p.IsActive).Sum(p => p.Amount) ?? 0;
                    decimal runningPaidAmount = totalCollected;

                    DateTime? earliestMissedDueDate = null;
                    for (int i = 0; i < d.TotalInstallments; i++)
                    {
                        DateTime dueDate = mode switch
                        {
                            "daily" => d.StartDate.AddDays(i),
                            "weekly" => d.StartDate.AddDays(i * 7),
                            "monthly" => d.StartDate.AddMonths(i),
                            _ => d.StartDate.AddMonths(i)
                        };

                        if (runningPaidAmount >= installmentAmount)
                        {
                            runningPaidAmount -= installmentAmount;
                        }
                        else
                        {
                            runningPaidAmount = 0;
                            if (earliestMissedDueDate == null && DateTime.Today > dueDate.Date)
                            {
                                earliestMissedDueDate = dueDate;
                            }
                        }
                    }

                    if (earliestMissedDueDate.HasValue)
                    {
                        int daysPastDue = (DateTime.Today - earliestMissedDueDate.Value.Date).Days;
                        if (daysPastDue > 90)
                        {
                            writeOff += Math.Max(0, d.PrincipalOffered - (totalCollected - interestCollected - penaltyCollected));
                        }
                    }
                }

                results.Add(new ProfitabilityReportDTO
                {
                    LoanType = loanType,
                    TotalLoansDisbursed = totalLoans,
                    TotalPrincipalDisbursed = totalDisbursed,
                    InterestIncomeCollected = interestCollected,
                    PenaltyIncomeCollected = penaltyCollected,
                    ProcessingFeesCollected = feesCollected,
                    WriteOffAmount = writeOff
                });
            }

            return results;
        }

        // ── Risk & Compliance ───────────────────────────────────────────────
        public async Task<List<CreditRiskReportDTO>> GetCreditRiskReportAsync()
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var disbursements = await dbContext.Disbursements
                .Include(d => d.LoanApplication)
                    .ThenInclude(la => la.Borrower)
                .Include(d => d.Payments)
                .Where(d => d.IsActive)
                .ToListAsync();

            var collaterals = await dbContext.Collaterals.ToListAsync();

            var results = new List<CreditRiskReportDTO>();

            foreach (var d in disbursements)
            {
                var loanCollaterals = collaterals.Where(c => c.LoanApplicationId == d.LoanApplicationId).ToList();
                decimal collateralValue = loanCollaterals.Sum(c => c.EstimatedValue);
                decimal outstandingBalance = Math.Max(0, d.Amount - d.Payments.Where(p => p.IsActive).Sum(p => p.Amount));

                decimal ltv = collateralValue > 0 ? (outstandingBalance / collateralValue) * 100 : 100;
                
                string riskStatus = "Low Risk";
                if (collateralValue == 0)
                {
                    riskStatus = "High Risk (Uncollateralized)";
                }
                else if (ltv > 80)
                {
                    riskStatus = "High Risk";
                }
                else if (ltv > 50)
                {
                    riskStatus = "Medium Risk";
                }

                results.Add(new CreditRiskReportDTO
                {
                    LoanId = d.LoanApplicationId,
                    ApplicationCode = d.LoanApplication.ApplicationCode,
                    BorrowerName = $"{d.LoanApplication.Borrower.FirstName} {d.LoanApplication.Borrower.LastName}",
                    OutstandingBalance = outstandingBalance,
                    CollateralValue = collateralValue,
                    LoanToValueRatio = ltv,
                    RiskStatus = riskStatus
                });
            }

            return results.OrderByDescending(r => r.LoanToValueRatio).ToList();
        }

        public async Task<List<NPLReportDTO>> GetNPLReportAsync()
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var disbursements = await dbContext.Disbursements
                .Include(d => d.LoanApplication)
                    .ThenInclude(la => la.Borrower)
                .Include(d => d.PaymentModality)
                .Include(d => d.Payments)
                .Where(d => d.IsActive)
                .ToListAsync();

            var results = new List<NPLReportDTO>();

            foreach (var d in disbursements)
            {
                if (d.TotalInstallments <= 0) continue;

                decimal installmentAmount = d.Amount / d.TotalInstallments;
                string mode = (d.PaymentModality?.Mode ?? "monthly").ToLower();
                decimal totalCollected = d.Payments?.Where(p => p.IsActive).Sum(p => p.Amount) ?? 0;
                decimal runningPaidAmount = totalCollected;

                DateTime? earliestMissedDueDate = null;

                for (int i = 0; i < d.TotalInstallments; i++)
                {
                    DateTime dueDate = mode switch
                    {
                        "daily" => d.StartDate.AddDays(i),
                        "weekly" => d.StartDate.AddDays(i * 7),
                        "monthly" => d.StartDate.AddMonths(i),
                        _ => d.StartDate.AddMonths(i)
                    };

                    if (runningPaidAmount >= installmentAmount)
                        runningPaidAmount -= installmentAmount;
                    else
                    {
                        runningPaidAmount = 0;
                        if (earliestMissedDueDate == null && DateTime.Today > dueDate.Date)
                        {
                            earliestMissedDueDate = dueDate;
                        }
                    }
                }

                if (earliestMissedDueDate.HasValue)
                {
                    int daysPastDue = (DateTime.Today - earliestMissedDueDate.Value.Date).Days;
                    if (daysPastDue >= 90)
                    {
                        string nplStatus = "Substandard";
                        if (daysPastDue > 360) nplStatus = "Loss (Default)";
                        else if (daysPastDue > 180) nplStatus = "Doubtful";

                        results.Add(new NPLReportDTO
                        {
                            LoanId = d.LoanApplicationId,
                            ApplicationCode = d.LoanApplication.ApplicationCode,
                            BorrowerName = $"{d.LoanApplication.Borrower.FirstName} {d.LoanApplication.Borrower.LastName}",
                            PrincipalAmount = d.PrincipalOffered,
                            OutstandingBalance = Math.Max(0, d.Amount - totalCollected),
                            DaysPastDue = daysPastDue,
                            NPLStatus = nplStatus
                        });
                    }
                }
            }

            return results.OrderByDescending(r => r.DaysPastDue).ToList();
        }

        public async Task<List<RegulatoryComplianceReportDTO>> GetRegulatoryComplianceReportAsync()
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var borrowers = await dbContext.Borrowers.ToListAsync();

            return borrowers.Select(b => {
                bool hasPhone = !string.IsNullOrWhiteSpace(b.PhoneNumber);
                bool hasEmail = !string.IsNullOrWhiteSpace(b.Email);
                bool hasAddress = !string.IsNullOrWhiteSpace(b.Village) && !string.IsNullOrWhiteSpace(b.District);
                bool hasId = !string.IsNullOrWhiteSpace(b.IdentificationNumber);
                
                string kycStatus = (hasId && hasPhone && hasAddress) ? "Compliant" : "Pending Verification";

                return new RegulatoryComplianceReportDTO
                {
                    BorrowerId = b.Id,
                    BorrowerName = $"{b.FirstName} {b.LastName}",
                    IdentificationNumber = b.IdentificationNumber ?? "Missing",
                    HasPhone = hasPhone,
                    HasEmail = hasEmail,
                    HasAddress = hasAddress,
                    KYCStatus = kycStatus
                };
            }).OrderBy(r => r.KYCStatus).ToList();
        }

        // ── Customer Reports ────────────────────────────────────────────────
        public async Task<List<CustomerPortfolioReportDTO>> GetCustomerPortfolioReportAsync()
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var borrowers = await dbContext.Borrowers.ToListAsync();
            var disbursements = await dbContext.Disbursements
                .Include(d => d.Payments)
                .ToListAsync();

            var results = new List<CustomerPortfolioReportDTO>();

            foreach (var b in borrowers)
            {
                var loanAppIds = await dbContext.LoanApplications
                    .Where(la => la.BorrowerId == b.Id)
                    .Select(la => la.Id)
                    .ToListAsync();

                var borrowerDisbursements = disbursements
                    .Where(d => loanAppIds.Contains(d.LoanApplicationId))
                    .ToList();

                int totalLoans = borrowerDisbursements.Count;
                int activeLoans = borrowerDisbursements.Count(d => d.IsActive);
                decimal totalDisbursed = borrowerDisbursements.Sum(d => d.PrincipalOffered);
                
                decimal totalPaid = borrowerDisbursements
                    .Sum(d => d.Payments?.Where(p => p.IsActive).Sum(p => p.Amount) ?? 0);

                decimal totalDue = borrowerDisbursements.Sum(d => d.Amount);
                decimal outstanding = Math.Max(0, totalDue - totalPaid);

                decimal payRatio = totalDue > 0 ? (totalPaid / totalDue) : 1;
                string creditRating = "Excellent";
                if (totalLoans > 0)
                {
                    if (payRatio < 0.6m) creditRating = "Poor";
                    else if (payRatio < 0.8m) creditRating = "Fair";
                    else if (payRatio < 0.95m) creditRating = "Good";
                }

                results.Add(new CustomerPortfolioReportDTO
                {
                    BorrowerId = b.Id,
                    BorrowerName = $"{b.FirstName} {b.LastName}",
                    TotalLoansCount = totalLoans,
                    ActiveLoansCount = activeLoans,
                    TotalDisbursed = totalDisbursed,
                    TotalPaid = totalPaid,
                    OutstandingBalance = outstanding,
                    CreditRiskRating = creditRating
                });
            }

            return results.OrderByDescending(r => r.OutstandingBalance).ToList();
        }

        public async Task<List<LoanApplicationStatusReportDTO>> GetLoanApplicationStatusReportAsync()
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var apps = await dbContext.LoanApplications
                .Include(la => la.Borrower)
                .Include(la => la.LoanProductSetting)
                    .ThenInclude(lps => lps.LoanProduct)
                .ToListAsync();

            return apps.Select(a => {
                int days = (DateTime.Now - a.DateofApplication).Days;
                return new LoanApplicationStatusReportDTO
                {
                    ApplicationId = a.Id,
                    ApplicationCode = a.ApplicationCode,
                    BorrowerName = $"{a.Borrower.FirstName} {a.Borrower.LastName}",
                    LoanType = a.LoanProductSetting.LoanProduct.ProductName,
                    AmountRequested = a.AmountRequested,
                    DateOfApplication = a.DateofApplication,
                    Status = a.Status.ToString(),
                    ApprovedBy = string.IsNullOrWhiteSpace(a.ApprovedBy) ? "N/A" : a.ApprovedBy,
                    ProcessingDays = days
                };
            }).OrderByDescending(r => r.DateOfApplication).ToList();
        }

        public async Task<List<CustomerRiskProfileReportDTO>> GetCustomerRiskProfileReportAsync()
        {
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var borrowers = await dbContext.Borrowers.ToListAsync();
            var disbursements = await dbContext.Disbursements
                .Include(d => d.Payments)
                .Include(d => d.PaymentModality)
                .ToListAsync();

            var results = new List<CustomerRiskProfileReportDTO>();

            foreach (var b in borrowers)
            {
                var loanAppIds = await dbContext.LoanApplications
                    .Where(la => la.BorrowerId == b.Id)
                    .Select(la => la.Id)
                    .ToListAsync();

                var borrowerDisbursements = disbursements
                    .Where(d => loanAppIds.Contains(d.LoanApplicationId))
                    .ToList();

                decimal totalDisbursed = borrowerDisbursements.Sum(d => d.PrincipalOffered);
                decimal totalDue = borrowerDisbursements.Sum(d => d.Amount);
                decimal totalPaid = borrowerDisbursements.Sum(d => d.Payments?.Where(p => p.IsActive).Sum(p => p.Amount) ?? 0);
                decimal outstanding = Math.Max(0, totalDue - totalPaid);

                int onTimePayments = 0;
                int latePayments = 0;

                foreach (var d in borrowerDisbursements)
                {
                    if (d.TotalInstallments <= 0) continue;

                    decimal installmentAmount = d.Amount / d.TotalInstallments;
                    string mode = (d.PaymentModality?.Mode ?? "monthly").ToLower();
                    decimal totalCollected = d.Payments?.Where(p => p.IsActive).Sum(p => p.Amount) ?? 0;
                    decimal runningPaidAmount = totalCollected;

                    for (int i = 0; i < d.TotalInstallments; i++)
                    {
                        DateTime dueDate = mode switch
                        {
                            "daily" => d.StartDate.AddDays(i),
                            "weekly" => d.StartDate.AddDays(i * 7),
                            "monthly" => d.StartDate.AddMonths(i),
                            _ => d.StartDate.AddMonths(i)
                        };

                        if (runningPaidAmount >= installmentAmount)
                        {
                            onTimePayments++;
                            runningPaidAmount -= installmentAmount;
                        }
                        else
                        {
                            runningPaidAmount = 0;
                            if (DateTime.Today > dueDate.Date)
                            {
                                latePayments++;
                            }
                            else
                            {
                                onTimePayments++;
                            }
                        }
                    }
                }

                string riskRating = "Low";
                if (latePayments > 2) riskRating = "High";
                else if (latePayments > 0) riskRating = "Medium";

                results.Add(new CustomerRiskProfileReportDTO
                {
                    BorrowerId = b.Id,
                    BorrowerName = $"{b.FirstName} {b.LastName}",
                    TotalDisbursed = totalDisbursed,
                    OutstandingBalance = outstanding,
                    OnTimePaymentsCount = onTimePayments,
                    LatePaymentsCount = latePayments,
                    RiskRating = riskRating
                });
            }

            return results.OrderByDescending(r => r.LatePaymentsCount).ToList();
        }
    }
}
