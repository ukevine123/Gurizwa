using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;

        public UserContext(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
        }

        private ClaimsPrincipal? ClaimsPrincipal
        {
            get
            {
                // 1. Try HttpContext
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.User?.Identity != null)
                {
                    return httpContext.User;
                }

                // 2. Try AuthenticationStateProvider (Blazor interactive circuits)
                try
                {
                    var authStateProvider = _serviceProvider.GetService<AuthenticationStateProvider>();
                    if (authStateProvider != null)
                    {
                        var task = authStateProvider.GetAuthenticationStateAsync();
                        if (task.IsCompleted)
                        {
                            return task.Result?.User;
                        }
                        else
                        {
                            return task.GetAwaiter().GetResult()?.User;
                        }
                    }
                }
                catch
                {
                    // Fallback
                }

                return null;
            }
        }

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

        /// <inheritdoc />
        public int? ParentUserId =>
            int.TryParse(ClaimsPrincipal?.FindFirst("ParentUserId")?.Value, out var id) ? id : null;

        /// <inheritdoc />
        public bool IsSubUser => ParentUserId.HasValue;

        /// <inheritdoc />
        public async Task<List<int>> GetAllowedPersonIdsAsync()
        {
            var personIds = new List<int>();
            var myPersonId = PersonId;
            if (myPersonId.HasValue)
            {
                personIds.Add(myPersonId.Value);
            }

            // If the user is a manager (no ParentUserId), they can also see all their sub-users' data
            if (IsAuthenticated && !ParentUserId.HasValue && Id.HasValue)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<ApplicationDbContext>>();
                    if (dbContextFactory != null)
                    {
                        using var db = await dbContextFactory.CreateDbContextAsync();
                        var agentPersonIds = await db.Users
                            .Where(u => u.ParentUserId == Id.Value)
                            .Select(u => u.PersonId)
                            .ToListAsync();
                        personIds.AddRange(agentPersonIds);
                    }
                }
                catch
                {
                    // Fallback to self
                }
            }

            return personIds.Distinct().ToList();
        }

        /// <inheritdoc />
        public async Task<int?> GetSettingsPersonIdAsync()
        {
            // If Agent (has ParentUserId), retrieve the Manager's PersonId
            if (IsAuthenticated && ParentUserId.HasValue)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<ApplicationDbContext>>();
                    if (dbContextFactory != null)
                    {
                        using var db = await dbContextFactory.CreateDbContextAsync();
                        var parentUser = await db.Users
                            .Where(u => u.Id == ParentUserId.Value)
                            .Select(u => new { u.PersonId })
                            .FirstOrDefaultAsync();
                        if (parentUser != null)
                        {
                            return parentUser.PersonId;
                        }
                    }
                }
                catch
                {
                    // Fallback to self
                }
            }

            return PersonId;
        }
    }
}