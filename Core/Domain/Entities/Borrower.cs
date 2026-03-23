using Domain.ValueObjects;
namespace Domain.Entities
{
    public class Borrower
    {
        public int Id{get;set;}
        public string FirstName {get;set;}
        public string LastName {get;set;}
        public int BorrowerTypeId {get;set;}
        public Sex sex{get;set;}
        public DateTime DateOfBirth {get;set;}
        public string IdentificationNumber{get;set;}
        public string Email {get;set;}
        public string PhoneNumber {get;set;}
        public Maritalstatus Maritalstatus{get;set;}
        public string SpouceIdNumber {get;set;}
        public string SpouceName {get;set;}
        public string Province {get;set;}
        public string District {get;set;}
        public string Sector {get;set;}
        public string Cell {get;set;}
        public string Village {get;set;}
        public DateTime CreatedAt {get;set;}
        public string CreatedBy {get;set;}
        
        

    }
}