using System;
using System.Security.Claims;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components.Authorization;
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
        public bool IsAuthenticated => ClaimsPrincipal?.Identity?.IsAuthenticated ?? false;

        /// <inheritdoc />
        public string Email => ClaimsPrincipal?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

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
    }
}