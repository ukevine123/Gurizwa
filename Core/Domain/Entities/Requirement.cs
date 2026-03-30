using Domain.Entities;

namespace Domain.Entities
{
    public class Requirement
    {
        public int Id { get; set; }
        public int RequiredDocumentId { get; set; }
        public RequiredDocument RequiredDocument {get;set;}
        public int? LoanProductId { get; set; }
        public LoanProduct LoanProduct {get;set;}

      
    }
}
