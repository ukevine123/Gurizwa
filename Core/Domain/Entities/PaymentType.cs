namespace Domain.Entities
{
    public class PaymentType
    {
        public int Id { get; set; }
        public string? PaymentTypeName { get; set; }
        public bool IsActive { get; set; }
         public Person Person {get;set;}
         public int PersonId {get;set;}
         public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
}