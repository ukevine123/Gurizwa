using System.Security.Claims;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Identity
{
   
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? ClaimsPrincipal => _httpContextAccessor.HttpContext?.User;

        /// <inheritdoc />
        public int? Id
        {
            get
            {
                var userIdClaim = ClaimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out var userId))
                {
                    return userId;
                }
                return null;
            }
        }

        /// <inheritdoc />
        public int? PersonId
        {
            get
            {
                var personIdClaim = ClaimsPrincipal?.FindFirst("PersonId")?.Value;
                if (int.TryParse(personIdClaim, out var personId))
                {
                    return personId;
                }
                return null;
            }
        }

        /// <inheritdoc />
        public bool IsAuthenticated => ClaimsPrincipal?.Identity?.IsAuthenticated ?? false;

        /// <inheritdoc />
        public string Email => ClaimsPrincipal?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

        /// <inheritdoc />
        public IList<string> Roles => ClaimsPrincipal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();

        /// <inheritdoc />
        public string PrimaryRole
        {
            get
            {
                var role = Roles.FirstOrDefault() ?? "Tenant";
                if (role.Contains("_"))
                {
                    return role.Substring(role.IndexOf("_") + 1);
                }
                return role;
            }
        }

        /// <inheritdoc />
        public string FirstName => ClaimsPrincipal?.FindFirst("FirstName")?.Value ?? string.Empty;

        /// <inheritdoc />
        public string LastName => ClaimsPrincipal?.FindFirst("LastName")?.Value ?? string.Empty;

        /// <inheritdoc />
        public string FullName => $"{FirstName} {LastName}".Trim();

        /// <inheritdoc />
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
}