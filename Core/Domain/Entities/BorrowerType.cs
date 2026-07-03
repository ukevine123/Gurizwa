namespace Domain.Entities
{
    public class BorrowerType
    {
        public int Id {get;set;}
        public string Type {get;set;}
        public string  Status {get;set;}
        public Person Person {get;set;}
        public int PersonId {get;set;}
    }
}