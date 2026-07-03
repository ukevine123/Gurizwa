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

        // Repayment & Collection
        Task<List<RepaymentScheduleReportDTO>> GetRepaymentScheduleReportAsync();
        Task<List<OverdueReportDTO>> GetOverdueReportAsync();
        Task<List<CollectionEfficiencyReportDTO>> GetCollectionEfficiencyReportAsync();

        // Financial Performance
        Task<List<InterestIncomeReportDTO>> GetInterestIncomeReportAsync(DateTime startDate, DateTime endDate);
        Task<List<PenaltyChargesReportDTO>> GetPenaltyChargesReportAsync(DateTime startDate, DateTime endDate);
        Task<List<ProfitabilityReportDTO>> GetProfitabilityReportAsync();

        // Risk & Compliance
        Task<List<CreditRiskReportDTO>> GetCreditRiskReportAsync();
        Task<List<NPLReportDTO>> GetNPLReportAsync();
        Task<List<RegulatoryComplianceReportDTO>> GetRegulatoryComplianceReportAsync();

        // Customer Reports
        Task<List<CustomerPortfolioReportDTO>> GetCustomerPortfolioReportAsync();
        Task<List<LoanApplicationStatusReportDTO>> GetLoanApplicationStatusReportAsync();
        Task<List<CustomerRiskProfileReportDTO>> GetCustomerRiskProfileReportAsync();
    }
}
