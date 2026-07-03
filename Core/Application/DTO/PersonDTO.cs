namespace Application.DTO
{
    public class PersonCreateDTO
    {
         public string FirstName{get;set;}
        public string LastName{get;set;}
        public string Sex{get;set;}
        public DateTime DateOfBirth{get;set;}
        public string phoneNumber{get;set;}
        public string Email{get;set;}
        public string Country{get;set;}
        public string City{get;set;}
        public string Street{get;set;}
        public string Status{get;set;}
       
       

    }
    public class PersonUpdateDTO
    {
        
         public string FirstName{get;set;}
        public string LastName{get;set;}
        public string Sex{get;set;}
        public string Country{get;set;}
        public string City{get;set;}
        public string Street{get;set;}
     
      
    }
}