namespace Application.DTO
{
    public class TransactionHistoryDTO
    {
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
