using Domain.Entities;

namespace Domain.Entities
{
    public class LoanProductSetting
    {
        public int Id { get; set; }
        public int LoanProductId { get; set; }
        public LoanProduct LoanProduct { get; set; }
        public decimal InterestRate {get;set;}
        public decimal ProcessingFee {get;set;}
        public decimal GracePeriodDays {get;set;}

    }
}