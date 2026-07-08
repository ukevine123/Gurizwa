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

        // Sub-user support — null means this is a top-level Manager account
        public int? ParentUserId { get; set; }
        public User ParentUser { get; set; }
        public ICollection<User> SubUsers { get; set; } = new List<User>();

        // Auditing fields
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Admin Approval for self-registered users
        public bool IsApproved { get; set; } = false;
    }
}