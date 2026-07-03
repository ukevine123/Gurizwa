namespace Domain.Entities
{
    public class PaymentModality
    {
        public int Id {get;set;}
        public string Mode {get;set;}
        public DateTime CreatedAt {get;set;}
        public string CreatedBy {get;set;}
        
    }
}