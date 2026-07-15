namespace Domain.Entities
{
    public class GuarantorType
    {
        public int Id {get;set;}
        public string? Name {get;set;}
        public string? Status {get;set;}
        public Person Person {get;set;}
        public int PersonId {get;set;}
        public DateTime CreatedAt {get;set;}
        public string? UpdatedBy {get;set;}
        
    }
}