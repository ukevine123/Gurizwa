using System;

namespace Application.DTO
{
    public class ExpenseDTO
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        public int? AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        
        public int PersonId { get; set; }
        
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
