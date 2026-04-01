using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity
{
    public static class Authentication
    {
         public static IServiceCollection AddAuthenticationService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultScheme = IdentityConstants.ApplicationScheme;

            }).AddIdentityCookies();

            services.AddIdentityCore<User>(options =>
            {
                // Password requirements - Make these match what you tell users!
                options.Password.RequireDigit = false;           // No digit required
                options.Password.RequiredLength = 8;             // Minimum 8 characters
                options.Password.RequireNonAlphanumeric = false; // No special characters required
                options.Password.RequireUppercase = false;       // No uppercase required (THIS WAS BLOCKING REGISTRATIONS)
                options.Password.RequireLowercase = false;       // No lowercase required

                // User settings
                options.User.RequireUniqueEmail = true;
                
                // IMPORTANT: Set to false for simple registration
                options.SignIn.RequireConfirmedAccount = false; // Allow immediate login after registration
                
                // Lockout settings
                options.Lockout.MaxFailedAccessAttempts = 5;    
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
              .AddRoles<IdentityRole<int>>()
              .AddEntityFrameworkStores<ApplicationDbContext>()
              .AddSignInManager()
              .AddDefaultTokenProviders()
            .AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>();

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromMinutes(3);
            });

            services.ConfigureApplicationCookie(options =>
            {
              options.Cookie.Name = "DigitalLoan";
              options.Cookie.HttpOnly = true;
              options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
              options.Cookie.SameSite = SameSiteMode.Lax;
            //   options.LoginPath = "/account/login";
            // //   options.LogoutPath = "/account/logout";
            // //   options.AccessDeniedPath = "/access-denied";
            });

            return services;
        }
    }
}