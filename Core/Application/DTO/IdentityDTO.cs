using System.ComponentModel.DataAnnotations;
using Domain.ValueObjects;

namespace Application.DTO
{
    public class RegisterUserDTO : IValidatableObject
    {
         public string FirstName { get; set; } = string.Empty;
         public string LastName { get; set; } = string.Empty;
         [Required(ErrorMessage = "Email is required.")]
         [EmailAddress(ErrorMessage = "Invalid email Address.")]
         public string Email { get; set; } = string.Empty;
         public string Status { get; set; } = string.Empty; 
         public DateTime? DateOfBirth { get; set; }
         public string Country { get; set; }
         public string City { get; set; }
         public string Street { get; set; }
         public Sex Sex { get; set; }

        
        public string PhoneNumber { get; set; }
        //biterwa na application urigisteruserdto
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
         public string Password { get; set; } = string.Empty;

         public string Role { get; set;}
         public int PersonId { get; set; }

         // Organization specific fields
         public string TenantType { get; set; } = "Personal"; // "Personal" or "Organization"
         public string CompanyName { get; set; } = string.Empty;
         public string TinNumber { get; set; } = string.Empty;
         public string ContactPerson { get; set; } = string.Empty;
         public int? TenantId { get; set; }

         public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
         {
             if (TenantType == "Personal")
             {
                 if (string.IsNullOrWhiteSpace(FirstName))
                     yield return new ValidationResult("First name is required.", new[] { nameof(FirstName) });
                 if (string.IsNullOrWhiteSpace(LastName))
                     yield return new ValidationResult("Last name is required.", new[] { nameof(LastName) });
                 if (!DateOfBirth.HasValue)
                     yield return new ValidationResult("Date of birth is required.", new[] { nameof(DateOfBirth) });
             }
             else if (TenantType == "Organization")
             {
                 if (string.IsNullOrWhiteSpace(CompanyName))
                     yield return new ValidationResult("Company name is required.", new[] { nameof(CompanyName) });
                 if (string.IsNullOrWhiteSpace(TinNumber))
                     yield return new ValidationResult("TIN number is required.", new[] { nameof(TinNumber) });
                 if (string.IsNullOrWhiteSpace(ContactPerson))
                     yield return new ValidationResult("Contact person is required.", new[] { nameof(ContactPerson) });
                 if (string.IsNullOrWhiteSpace(PhoneNumber))
                     yield return new ValidationResult("Phone number is required.", new[] { nameof(PhoneNumber) });
             }
         }
    }
     public class UserDetailDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Role { get; set; }

        // Sub-user support
        public int? ParentUserId { get; set; }
        public bool IsActive { get; set; } = true;

        /// <summary>Gets the full name (FirstName LastName)</summary>
        public string FullName => $"{FirstName} {LastName}".Trim();

        /// <summary>Gets the avatar initials</summary>
        public string Initials
        {
            get
            {
                var first = !string.IsNullOrEmpty(FirstName) ? FirstName[0].ToString().ToUpper() : "";
                var last  = !string.IsNullOrEmpty(LastName)  ? LastName[0].ToString().ToUpper()  : "";
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

    /// <summary>
    /// Data required to create a new sub-user (Agent) under a parent Manager.
    /// </summary>
    public class CreateSubUserDTO
    {
        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = string.Empty;
    }

    public class SetupTenantUserDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

}