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
    }
}
