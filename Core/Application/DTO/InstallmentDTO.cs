namespace Application.DTO
{
    public class InstallmentDTO
    {
        public int InstallmentNumber { get; set; } // Based on your TotalInstallments (1, 2, 3...)
        public DateTime DueDate { get; set; }
        public decimal AmountDue { get; set; }      // The original amount for this slot
        public decimal RemainingAmount { get; set; } // This reduces as you pay
        public string Status { get; set; }          // "Paid", "Partial", or "Pending"
    }
}