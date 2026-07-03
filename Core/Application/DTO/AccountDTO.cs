namespace Application.DTO
{
    public class AccountCreateDTO
    {
       
        public string Name { get; set; }
        public string Provider { get; set; }
        public int AccountTypeId { get; set; }
        public string AccountNumber{ get; set;}
        public decimal Balance { get; set;}
        public string Currency { get; set; } = string.Empty;
    }
}