using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTO;
using Application.Interfaces;

namespace Application.Service.Reports
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<List<ActiveLoanSummaryDTO>> GetActiveLoansSummaryAsync()
        {
            return await _reportRepository.GetActiveLoansSummaryAsync();
        }

        public async Task<List<LoanDisbursementReportDTO>> GetLoanDisbursementReportAsync(DateTime startDate, DateTime endDate, string? loanType = null)
        {
            return await _reportRepository.GetLoanDisbursementReportAsync(startDate, endDate, loanType);
        }

        public async Task<List<LoanMaturityReportDTO>> GetLoanMaturityReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            return await _reportRepository.GetLoanMaturityReportAsync(startDate, endDate);
        }

        public async Task<List<UserActivityReportDTO>> GetUserActivityReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            return await _reportRepository.GetUserActivityReportAsync(startDate, endDate);
        }

        public async Task<List<ActivityHeatmapDTO>> GetActivityHeatmapAsync(int days = 365)
        {
            return await _reportRepository.GetActivityHeatmapAsync(days);
        }

        // ── Repayment & Collection ─────────────────────────────────────────────
        public async Task<List<RepaymentScheduleReportDTO>> GetRepaymentScheduleReportAsync() => await _reportRepository.GetRepaymentScheduleReportAsync();
        public async Task<List<OverdueReportDTO>> GetOverdueReportAsync() => await _reportRepository.GetOverdueReportAsync();
        public async Task<List<CollectionEfficiencyReportDTO>> GetCollectionEfficiencyReportAsync() => await _reportRepository.GetCollectionEfficiencyReportAsync();

        // ── Financial Performance ──────────────────────────────────────────────
        public async Task<List<InterestIncomeReportDTO>> GetInterestIncomeReportAsync() => await _reportRepository.GetInterestIncomeReportAsync();
        public async Task<List<PenaltyIncomeReportDTO>> GetPenaltyIncomeReportAsync() => await _reportRepository.GetPenaltyIncomeReportAsync();
        public async Task<List<ProfitabilityReportDTO>> GetProfitabilityReportAsync() => await _reportRepository.GetProfitabilityReportAsync();
        public async Task<IncomeStatementReportDTO> GetIncomeStatementAsync(DateTime startDate, DateTime endDate) => await _reportRepository.GetIncomeStatementAsync(startDate, endDate);

        // ── Risk & Compliance ──────────────────────────────────────────────────
        public async Task<List<CreditRiskReportDTO>> GetCreditRiskReportAsync() => await _reportRepository.GetCreditRiskReportAsync();
        public async Task<List<NplReportDTO>> GetNplReportAsync() => await _reportRepository.GetNplReportAsync();
        public async Task<List<RegulatoryComplianceReportDTO>> GetRegulatoryComplianceReportAsync() => await _reportRepository.GetRegulatoryComplianceReportAsync();

        // ── Customer Reports ───────────────────────────────────────────────────
        public async Task<List<CustomerPortfolioReportDTO>> GetCustomerPortfolioReportAsync() => await _reportRepository.GetCustomerPortfolioReportAsync();
        public async Task<List<ApplicationStatusReportDTO>> GetApplicationStatusReportAsync() => await _reportRepository.GetApplicationStatusReportAsync();
        public async Task<List<CustomerRiskProfileReportDTO>> GetCustomerRiskProfileReportAsync() => await _reportRepository.GetCustomerRiskProfileReportAsync();
    }
}
