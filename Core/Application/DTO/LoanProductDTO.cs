namespace Application.DTO
{
    public class LoanProductCreateDTO
    {
       
        public string? ProductName { get; set; }
        public decimal InterestRate {get;set;}
        public string Description {get;set;}
       
    }

    public class LoanProductUpdateDTO
    {
        public string? ProductName { get; set; }
        public decimal InterestRate {get;set;}
        public string Description {get;set;}
        
        
    }


    
}
        