namespace Domain.Entities
{
    public class Reason
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public Person Person {get;set;}
        public int PersonId {get;set;}
    }
}