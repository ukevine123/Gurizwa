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
    }
}
