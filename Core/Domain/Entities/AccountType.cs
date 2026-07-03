using Domain.Entities;

namespace Domain.Entities
{
    public class AccountType
    {
        public int Id { get; set; }
        public string? AccountTypeName { get; set; }
        public Person Person {get;set;}
        public int PersonId {get;set;}
    }
}