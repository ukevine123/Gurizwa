using Domain.Entities;

namespace Application.DTO
{
    public class CollateralCreateDTO
    {
       public int LoanApplicationId { get; set; }
        public string AssetName { get; set; }
        public string AssetType { get; set; }
        public string Province {get;set;}
        public string District {get;set;}
        public string Sector {get;set;}
        public string Cell {get;set;}
        public string Village {get;set;}
        public decimal EstimatedValue { get; set; }
        public string IdentificationNumber { get; set; }
        public string Description { get; set; }
        public string ValuerName { get; set; }
        public DateTime? ValuationDate { get; set; }
    }

      public class CollateralUpdateDTO{
        public string AssetName { get; set; }
        public string AssetType { get; set; }
         public string Province {get;set;}
        public string District {get;set;}
        public string Sector {get;set;}
        public string Cell {get;set;}
        public string Village {get;set;}
        public decimal EstimatedValue { get; set; }
        public string IdentificationNumber { get; set; }
        public string Description { get; set; }
        public string ValuerName { get; set; }
        public DateTime? ValuationDate { get; set; }
}
    
}