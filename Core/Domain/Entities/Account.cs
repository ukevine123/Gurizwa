using Domain.Entities;

namespace Domain.Entities
{
    public class Account
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Provider { get; set; }
        public Person Person {get;set;}
        public int PersonId {get;set;}
        public int AccountTypeId { get; set; }
        public AccountType AccountType { get; set; }
        public string AccountNumber{ get; set;}
        public decimal Balance { get; set;}
        public string Currency { get; set;}

    }
}