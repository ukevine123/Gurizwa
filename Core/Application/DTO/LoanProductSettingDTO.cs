using Domain.Entities;

namespace Application.DTO
{
    public class CreateLoanProductSettingDTO
    {
       
        
        public int LoanProductId { get; set; }
        public LoanProduct LoanProduct { get; set; }
        public decimal InterestRate {get;set;}
        public decimal ProcessingFee {get;set;}
        public decimal GracePeriodDays {get;set;}

    }

        public class UpdateLoanProductSettingDTO
    {
        public decimal InterestRate {get;set;}
        public decimal ProcessingFee {get;set;}
        public decimal GracePeriodDays {get;set;}

    }

    
}