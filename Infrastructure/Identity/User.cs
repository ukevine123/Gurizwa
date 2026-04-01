using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity
{
    public class User : IdentityUser<int>
    {
        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int PersonId { get; set; }
        public Person Person { get; set; } 

        //Auditing fields
        public DateTime CreatedAt { get; set; } =DateTime.Now;
        public DateTime UpdatedAt { get; set; } =DateTime.Now;

    }
}