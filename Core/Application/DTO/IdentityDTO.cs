using System.ComponentModel.DataAnnotations;

namespace Application.DTO
{
    public class RegisterUserDTO
    {
        [Required(ErrorMessage = "First name is required.")]
         public string FirstName { get; set; } = string.Empty;
       [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email is required.")]
         [EmailAddress(ErrorMessage = "Invalid email Address.")]
         public string Email { get; set; } = string.Empty;
         public string Status { get; set; } = string.Empty; 
         public DateTime DateOfBirth { get; set; }
         public string Country { get; set; }
         public string City { get; set; }
         public string Street { get; set; }
         public string Sex { get; set; }

        
        public string PhoneNumber { get; set; }
        //biterwa na application urigisteruserdto
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
         public string Password { get; set; } = string.Empty;

         public string Role { get; set;}
         public int PersonId { get; set; }

    }
     public class UserDetailDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
       public  bool EmailConfirmed { get; set; }
       public DateTime CreatedAt { get; set; }
       public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets the full name (FirstName LastName)
        /// </summary>
        public string FullName => $"{FirstName} {LastName}".Trim();

        /// <summary>
        /// Gets the avatar initials (first letter of first name + first letter of last name)
        /// </summary>
        public string Initials
        {
            get
            {
                var first = !string.IsNullOrEmpty(FirstName) ? FirstName[0].ToString().ToUpper() : "";
                var last = !string.IsNullOrEmpty(LastName) ? LastName[0].ToString().ToUpper() : "";
                return $"{first}{last}";
    }
        }
    }   

    public class UpdateUserDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName{get; set; }
        public string PhoneNumber { get; set; }
        //biterwa na application urigisteruserdto
        public string Password { get; set; }

        public string Role { get; set;}
    }

    public class LoginDTO
    {
        [Required(ErrorMessage = "Email is required.")]
         [EmailAddress(ErrorMessage = "Invalid email Address.")]
         public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
    
      
}