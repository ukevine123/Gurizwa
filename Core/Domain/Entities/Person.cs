

namespace Domain.Entities
{
    public class Person
    {
        public int Id {get;set;}
        public string FirstName{get;set;}
        public string LastName{get;set;}
        public string Sex{get;set;}
        public DateTime DateOfBirth{get;set;}
        public string phoneNumber{get;set;}
        public string Email{get;set;}
        public string Country{get;set;}
        public string Status{get;set;}

            //mult tenant 
        public List<Account> Accounts {get;set;}
        // public List<LoanApplication> LoanApplications {get;set;}

        public string CreatedBy {get;set;}
        public DateTime CreatedAt {get;set;}
        public string UpdateBy {get;set;}
         

    

    }
}