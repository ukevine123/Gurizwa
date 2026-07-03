using System;

namespace Application.DTO
{
    public class ActiveLoanSummaryDTO
    {
        public int LoanId { get; set; }
        public string ApplicationCode { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public decimal PrincipalBalance { get; set; }
        public decimal InterestRate { get; set; }
        public int TenureMonths { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class LoanDisbursementReportDTO
    {
        public int LoanId { get; set; }
        public string ApplicationCode { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public decimal AmountDisbursed { get; set; }
        public string LoanType { get; set; } = string.Empty;
        public string Branch { get; set; } = "Main Branch"; // Placeholder if branch not in schema
        public string Officer { get; set; } = string.Empty;
        public DateTime DisbursementDate { get; set; }
    }

    public class LoanMaturityReportDTO
    {
        public int LoanId { get; set; }
        public string ApplicationCode { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public DateTime MaturityDate { get; set; }
        public decimal RemainingBalance { get; set; }
        public string Status { get; set; } = string.Empty; // e.g., "Nearing Completion", "Matured"
    }

    public class UserActivityReportDTO
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    // ── Repayment Schedule Report ──────────────────────────────────────────
    public class RepaymentScheduleReportDTO
    {
        public int LoanId { get; set; }
        public string ApplicationCode { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public string LoanType { get; set; } = string.Empty;
        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TotalInstallments { get; set; }
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal InstallmentAmount { get; set; }   // Expected payment per instalment
        public decimal AmountPaid { get; set; }          // Actual paid for this period
        public decimal Balance { get; set; }             // Remaining principal balance
        public string Status { get; set; } = string.Empty; // Paid | Partial | Pending | Overdue
    }

    // ── Overdue / Delinquency Report ──────────────────────────────────────
    public class OverdueReportDTO
    {
        public int LoanId { get; set; }
        public string ApplicationCode { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public string LoanType { get; set; } = string.Empty;
        public decimal PrincipalAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
        public DateTime DueDate { get; set; }            // The earliest missed due date
        public int DaysPastDue { get; set; }
        public decimal OverdueAmount { get; set; }       // Total amount overdue (not yet paid)
        public string RiskCategory { get; set; } = string.Empty; // 1-30 | 31-60 | 61-90 | 90+
    }

    // ── Collection Efficiency Report ──────────────────────────────────────
    public class CollectionEfficiencyReportDTO
    {
        public int LoanId { get; set; }
        public string ApplicationCode { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public string LoanType { get; set; } = string.Empty;
        public decimal TotalDue { get; set; }            // Sum of all instalments due so far
        public decimal TotalCollected { get; set; }      // Sum of all payments received
        public decimal OutstandingBalance { get; set; }  // Principal yet to be paid
        public decimal CollectionRate { get; set; }      // TotalCollected / TotalDue * 100
        public string EfficiencyCategory { get; set; } = string.Empty; // Excellent | Good | Fair | Poor
    }

    // ── Interest Income Report ──────────────────────────────────────────────
    public class InterestIncomeReportDTO
    {
        public int LoanId { get; set; }
        public string ApplicationCode { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public string LoanType { get; set; } = string.Empty;
        public decimal InterestRate { get; set; }
        public decimal TotalInterestExpected { get; set; }
        public decimal InterestCollected { get; set; }
        public decimal InterestOutstanding => Math.Max(0, TotalInterestExpected - InterestCollected);
    }

    // ── Penalty / Charges Report ────────────────────────────────────────────
    public class PenaltyChargesReportDTO
    {
        public int LoanId { get; set; }
        public string ApplicationCode { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public decimal PenaltyAmountLevied { get; set; }
        public decimal PenaltyPaid { get; set; }
        public decimal PenaltyOutstanding => Math.Max(0, PenaltyAmountLevied - PenaltyPaid);
        public DateTime PenaltyDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    // ── Profitability Report ────────────────────────────────────────────────
    public class ProfitabilityReportDTO
    {
        public string LoanType { get; set; } = string.Empty;
        public int TotalLoansDisbursed { get; set; }
        public decimal TotalPrincipalDisbursed { get; set; }
        public decimal InterestIncomeCollected { get; set; }
        public decimal PenaltyIncomeCollected { get; set; }
        public decimal ProcessingFeesCollected { get; set; }
        public decimal TotalRevenue => InterestIncomeCollected + PenaltyIncomeCollected + ProcessingFeesCollected;
        public decimal WriteOffAmount { get; set; } // Principal of defaulted (NPL Loss) loans
        public decimal NetProfit => TotalRevenue - WriteOffAmount;
    }

    // ── Credit Risk Report ──────────────────────────────────────────────────
    public class CreditRiskReportDTO
    {
        public int LoanId { get; set; }
        public string ApplicationCode { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public decimal OutstandingBalance { get; set; }
        public decimal CollateralValue { get; set; }
        public decimal LoanToValueRatio { get; set; } // (OutstandingBalance / CollateralValue) * 100
        public string RiskStatus { get; set; } = string.Empty; // Low Risk | Medium Risk | High Risk
    }

    // ── Non-Performing Loans (NPL) Report ──────────────────────────────────
    public class NPLReportDTO
    {
        public int LoanId { get; set; }
        public string ApplicationCode { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public decimal PrincipalAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
        public int DaysPastDue { get; set; }
        public string NPLStatus { get; set; } = string.Empty; // Substandard (90-180) | Doubtful (181-360) | Loss (360+)
    }

    // ── Regulatory Compliance Report ────────────────────────────────────────
    public class RegulatoryComplianceReportDTO
    {
        public int BorrowerId { get; set; }
        public string BorrowerName { get; set; } = string.Empty;
        public string IdentificationNumber { get; set; } = string.Empty;
        public bool HasId => !string.IsNullOrWhiteSpace(IdentificationNumber);
        public bool HasPhone { get; set; }
        public bool HasEmail { get; set; }
        public bool HasAddress { get; set; }
        public string KYCStatus { get; set; } = string.Empty; // Compliant | Pending Verification
    }

    // ── Customer Portfolio Report ───────────────────────────────────────────
    public class CustomerPortfolioReportDTO
    {
        public int BorrowerId { get; set; }
        public string BorrowerName { get; set; } = string.Empty;
        public int TotalLoansCount { get; set; }
        public int ActiveLoansCount { get; set; }
        public decimal TotalDisbursed { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal OutstandingBalance { get; set; }
        public string CreditRiskRating { get; set; } = string.Empty; // Excellent | Good | Fair | Poor
    }

    // ── Loan Application Status Report ──────────────────────────────────────
    public class LoanApplicationStatusReportDTO
    {
        public int ApplicationId { get; set; }
        public string ApplicationCode { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public string LoanType { get; set; } = string.Empty;
        public decimal AmountRequested { get; set; }
        public DateTime DateOfApplication { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
        public int ProcessingDays { get; set; }
    }

    // ── Customer Risk Profile Report ────────────────────────────────────────
    public class CustomerRiskProfileReportDTO
    {
        public int BorrowerId { get; set; }
        public string BorrowerName { get; set; } = string.Empty;
        public decimal TotalDisbursed { get; set; }
        public decimal OutstandingBalance { get; set; }
        public int OnTimePaymentsCount { get; set; }
        public int LatePaymentsCount { get; set; }
        public string RiskRating { get; set; } = string.Empty; // Low | Medium | High
    }
}
