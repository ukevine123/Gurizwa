namespace Domain.Entities
{
    public class Address
    {
        public int Id {get;set;}
        public string Province {get;set;}
        public string District {get;set;}
        public string Sector {get;set;}
        public string Cell {get;set;}
        public string Village {get;set;}
        public DateTime CreatedAt {get;set;}
        public string CreatedBy {get;set;}
    }
}