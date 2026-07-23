using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTO;

namespace Application.Interfaces
{
    public interface IReportService
    {
        Task<List<ActiveLoanSummaryDTO>> GetActiveLoansSummaryAsync();
        Task<List<LoanDisbursementReportDTO>> GetLoanDisbursementReportAsync(DateTime startDate, DateTime endDate, string? loanType = null);
        Task<List<LoanMaturityReportDTO>> GetLoanMaturityReportAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<UserActivityReportDTO>> GetUserActivityReportAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<ActivityHeatmapDTO>> GetActivityHeatmapAsync(int days = 365);
        
        // Repayment & Collection
        Task<List<RepaymentScheduleReportDTO>> GetRepaymentScheduleReportAsync();
        Task<List<OverdueReportDTO>> GetOverdueReportAsync();
        Task<List<CollectionEfficiencyReportDTO>> GetCollectionEfficiencyReportAsync();
        
        // Financial Performance
        Task<List<InterestIncomeReportDTO>> GetInterestIncomeReportAsync();
        Task<List<PenaltyIncomeReportDTO>> GetPenaltyIncomeReportAsync();
        Task<List<ProfitabilityReportDTO>> GetProfitabilityReportAsync();
        Task<IncomeStatementReportDTO> GetIncomeStatementAsync(DateTime startDate, DateTime endDate);
        
        // Risk & Compliance
        Task<List<CreditRiskReportDTO>> GetCreditRiskReportAsync();
        Task<List<NplReportDTO>> GetNplReportAsync();
        Task<List<RegulatoryComplianceReportDTO>> GetRegulatoryComplianceReportAsync();
        
        // Customer Reports
        Task<List<CustomerPortfolioReportDTO>> GetCustomerPortfolioReportAsync();
        Task<List<ApplicationStatusReportDTO>> GetApplicationStatusReportAsync();
        Task<List<CustomerRiskProfileReportDTO>> GetCustomerRiskProfileReportAsync();
        
        // Loan Products
        Task<List<LoanProductTrackerDTO>> GetLoanProductTrackerAsync();

        // Account History
        Task<List<AccountHistoryReportDTO>> GetAccountHistoryReportAsync(DateTime startDate, DateTime endDate);
    }
}
