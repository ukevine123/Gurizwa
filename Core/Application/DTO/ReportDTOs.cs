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
        public string EntityId { get; set; } = string.Empty;
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

    // ── Financial Performance Reports ─────────────────────────────────────
    public class InterestIncomeReportDTO
    {
        public string Period { get; set; } = string.Empty;
        public decimal TotalInterestExpected { get; set; }
        public decimal TotalInterestCollected { get; set; }
    }

    public class PenaltyIncomeReportDTO
    {
        public int LoanId { get; set; }
        public string ApplicationCode { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public decimal TotalPenalties { get; set; }
        public decimal CollectedPenalties { get; set; }
    }

    public class ProfitabilityReportDTO
    {
        public string LoanType { get; set; } = string.Empty;
        public int TotalLoans { get; set; }
        public decimal TotalPrincipal { get; set; }
        public decimal TotalInterestEarned { get; set; }
        public decimal TotalFeesEarned { get; set; }
        public decimal NetProfit { get; set; }
    }

    // ── Risk and Compliance Reports ───────────────────────────────────────
    public class CreditRiskReportDTO
    {
        public int BorrowerId { get; set; }
        public string BorrowerName { get; set; } = string.Empty;
        public int ActiveLoans { get; set; }
        public decimal TotalExposure { get; set; }
        public int MissedPayments { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
    }

    public class NplReportDTO
    {
        public int LoanId { get; set; }
        public string ApplicationCode { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public decimal OutstandingBalance { get; set; }
        public int DaysPastDue { get; set; }
        public string NplStatus { get; set; } = string.Empty;
    }

    public class RegulatoryComplianceReportDTO
    {
        public int BorrowerId { get; set; }
        public string BorrowerName { get; set; } = string.Empty;
        public bool KycComplete { get; set; }
        public bool AmlCleared { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    // ── Customer Reports ──────────────────────────────────────────────────
    public class CustomerPortfolioReportDTO
    {
        public int BorrowerId { get; set; }
        public string BorrowerName { get; set; } = string.Empty;
        public int ActiveLoans { get; set; }
        public int ClosedLoans { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal TotalPaid { get; set; }
    }

    public class ApplicationStatusReportDTO
    {
        public int ApplicationId { get; set; }
        public string ApplicationCode { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ApplicationDate { get; set; }
        public int ProcessingDays { get; set; }
    }

    public class CustomerRiskProfileReportDTO
    {
        public int BorrowerId { get; set; }
        public string BorrowerName { get; set; } = string.Empty;
        public string CreditScore { get; set; } = string.Empty;
        public int DefaultHistory { get; set; }
        public string RiskRating { get; set; } = string.Empty;
    }

    public class IncomeStatementReportDTO
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        // Income
        public decimal TotalInterestIncome { get; set; }
        public decimal TotalProcessingFeeIncome { get; set; }
        public decimal TotalPenaltyIncome { get; set; }
        public decimal GrossIncome => TotalInterestIncome + TotalProcessingFeeIncome + TotalPenaltyIncome;
        
        // Deductions / Expenses
        public decimal TotalWaiversAndWriteOffs { get; set; }
        public decimal TotalOperatingExpenses { get; set; }
        public decimal TotalDeductions => TotalWaiversAndWriteOffs + TotalOperatingExpenses;
        
        // Net
        public decimal NetIncome => GrossIncome - TotalDeductions;

        // Details
        public List<InterestIncomeDetailDTO> InterestDetails { get; set; } = new();
        public List<ProcessingFeeDetailDTO> ProcessingFeeDetails { get; set; } = new();
        public List<PenaltyIncomeDetailDTO> PenaltyDetails { get; set; } = new();
        public List<OperatingExpenseDetailDTO> OperatingExpenseDetails { get; set; } = new();
        public List<WaiverDetailDTO> WaiverDetails { get; set; } = new();
    }

    public class InterestIncomeDetailDTO
    {
        public string NameOfInterest { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string AccountReceiver { get; set; } = string.Empty;
        public DateTime DateOfInterest { get; set; }
    }

    public class ProcessingFeeDetailDTO
    {
        public string LoanApplicationCode { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string AccountReceiver { get; set; } = string.Empty;
    }

    public class PenaltyIncomeDetailDTO
    {
        public string LoanApplicationCode { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class OperatingExpenseDetailDTO
    {
        public string ExpenseName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string AccountFunding { get; set; } = string.Empty;
    }

    public class WaiverDetailDTO
    {
        public string LoanApplicationCode { get; set; } = string.Empty;
        public decimal AmountWaived { get; set; }
        public string WaiverType { get; set; } = string.Empty;
        public DateTime WaivingDate { get; set; }
    }

    public class LoanProductTrackerDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal InterestRate { get; set; }
        public decimal ProcessingFee { get; set; }
        public decimal PenaltyRate { get; set; }
        public int NumberOfAppliedBorrowers { get; set; }
        public decimal TotalInterestEarned { get; set; }
        public decimal TotalLosses { get; set; }
        public decimal TotalWaived { get; set; }
        public int TotalRescheduled { get; set; }
        public List<ProductBorrowerDTO> Borrowers { get; set; } = new();
    }

    public class ProductBorrowerDTO
    {
        public int BorrowerId { get; set; }
        public int LoanId { get; set; }
        public string BorrowerName { get; set; } = string.Empty;
        public string ApplicationCode { get; set; } = string.Empty;
        public string LoanStatus { get; set; } = string.Empty;
        public decimal PrincipalBalance { get; set; }
        public DateTime ApplicationDate { get; set; }
    }
}
