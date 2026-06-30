using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Infrastructure.Identity
{
    /// <summary>
    /// Custom claims principal factory that adds FirstName and LastName as claims.
    /// This makes user data available without additional database calls.
    /// </summary>
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, IdentityRole<int>>
    {
        public CustomUserClaimsPrincipalFactory(
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IOptions<IdentityOptions> options)
            : base(userManager, roleManager, options)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Add custom claims for FirstName and LastName
            identity.AddClaim(new Claim("FirstName", user.FirstName ?? string.Empty));
            identity.AddClaim(new Claim("LastName", user.LastName ?? string.Empty));

            // Stamp parent info so sub-user pages know who owns this account
            if (user.ParentUserId.HasValue)
                identity.AddClaim(new Claim("ParentUserId", user.ParentUserId.Value.ToString()));

            return identity;
        }
    }
}