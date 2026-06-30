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
}
