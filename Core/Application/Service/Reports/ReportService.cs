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

        // Repayment & Collection
        public async Task<List<RepaymentScheduleReportDTO>> GetRepaymentScheduleReportAsync()
        {
            return await _reportRepository.GetRepaymentScheduleReportAsync();
        }

        public async Task<List<OverdueReportDTO>> GetOverdueReportAsync()
        {
            return await _reportRepository.GetOverdueReportAsync();
        }

        public async Task<List<CollectionEfficiencyReportDTO>> GetCollectionEfficiencyReportAsync()
        {
            return await _reportRepository.GetCollectionEfficiencyReportAsync();
        }

        // Financial Performance
        public async Task<List<InterestIncomeReportDTO>> GetInterestIncomeReportAsync(DateTime startDate, DateTime endDate)
        {
            return await _reportRepository.GetInterestIncomeReportAsync(startDate, endDate);
        }

        public async Task<List<PenaltyChargesReportDTO>> GetPenaltyChargesReportAsync(DateTime startDate, DateTime endDate)
        {
            return await _reportRepository.GetPenaltyChargesReportAsync(startDate, endDate);
        }

        public async Task<List<ProfitabilityReportDTO>> GetProfitabilityReportAsync()
        {
            return await _reportRepository.GetProfitabilityReportAsync();
        }

        // Risk & Compliance
        public async Task<List<CreditRiskReportDTO>> GetCreditRiskReportAsync()
        {
            return await _reportRepository.GetCreditRiskReportAsync();
        }

        public async Task<List<NPLReportDTO>> GetNPLReportAsync()
        {
            return await _reportRepository.GetNPLReportAsync();
        }

        public async Task<List<RegulatoryComplianceReportDTO>> GetRegulatoryComplianceReportAsync()
        {
            return await _reportRepository.GetRegulatoryComplianceReportAsync();
        }

        // Customer Reports
        public async Task<List<CustomerPortfolioReportDTO>> GetCustomerPortfolioReportAsync()
        {
            return await _reportRepository.GetCustomerPortfolioReportAsync();
        }

        public async Task<List<LoanApplicationStatusReportDTO>> GetLoanApplicationStatusReportAsync()
        {
            return await _reportRepository.GetLoanApplicationStatusReportAsync();
        }

        public async Task<List<CustomerRiskProfileReportDTO>> GetCustomerRiskProfileReportAsync()
        {
            return await _reportRepository.GetCustomerRiskProfileReportAsync();
        }
    }
}
