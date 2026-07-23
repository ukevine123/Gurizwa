using Web.Components;
using MudBlazor.Services;
using Infrastructure.Identity;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Infrastructure.DependencyInjection;
using Application.Services.Borrowers;
using Application.Services.BorrowerTypes;
using Microsoft.Extensions.FileProviders;
using Application.Interfaces;
using Infrastructure.Services;
using Application.Services.LoanApplications;
using Application.Services.Guarantors;
using Application.Services.GuarantorTypes;
using Application.Services.ProvidedDocuments;
using Application.Services.PaymentModalities;
using Application.Services.Accounts;
using Application.Services.LoanProducts;
using Application.Services.RequiredDocuments;
using Application.Services.AccountTypes;
using Application.Services.Users;
using Application.Services.Persons;
using Microsoft.AspNetCore.Identity;
using Application.Services.Requirements;

using Application.Services.Disbursements;
using Application.Services.Reasons;
using Application.Services.Payments;
using Application.Services.Penalities;
using Application.Services.PaymentTypes;
using Application.Services.Collaterals;
using Application.Services.ProcessFeeDeposits;
using Application.Services.LoanProductSettings;
using Application.Service.Reports;
using Application.Services.Waivers;
using Application.Service;


var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVICES REGISTRATION ---

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("LoanPlatformDBCONN");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, x => {
        x.MigrationsAssembly("Infrastructure");
        x.UseCompatibilityLevel(120);
    }), ServiceLifetime.Scoped);

builder.Services.AddScoped(p => 
    p.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

// Identity Configuration (ONLY ONE REGISTRATION)
// Note: Ensure your 'User' class in Infrastructure matches IdentityRole<int> if that's your preference
builder.Services.AddIdentity<User, IdentityRole<int>>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>();

// Core Services
builder.Services.AddScoped<UserContext>();
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.SnackbarVariant = MudBlazor.Variant.Filled;
});
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddControllers(); // Moved up from the bottom
builder.Services.AddAuthorization(); // Moved up from the bottom

// Business Services
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddScoped<IBorrowerService, BorrowerService>();
builder.Services.AddScoped<IBorrowerTypeService, BorrowerTypeService>();
builder.Services.AddScoped<ILoanApplicationService, LoanApplicationService>();
builder.Services.AddScoped<IGuarantorService, GuarantorService>();
builder.Services.AddScoped<IGuarantorTypeService, GuarantorTypeService>();
builder.Services.AddScoped<IProvidedDocumentService, ProvidedDocumentService>();
builder.Services.AddScoped<IPaymentModalityService, PaymentModalityService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ILoanProductService, LoanProductService>();
builder.Services.AddScoped<IRequiredDocumentService, RequiredDocumentService>();
builder.Services.AddScoped<IAccountTypeService, AccountTypeService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IRequirementService, RequirementService>();

builder.Services.AddScoped<IDisbursementService, DisbursementService>();
builder.Services.AddScoped<IReasonService, ReasonService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPenalityService, PenalityService>();
builder.Services.AddScoped<ICollateralService, CollateralService>();

builder.Services.AddScoped<IPaymentTypeService, PaymentTypeService>();
builder.Services.AddScoped<IProcessFeeDepositService, ProcessFeeDepositService>();
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<ILoanProductSettingService, LoanProductSettingService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IWaiverService, WaiverService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IQRCodeService, QRCodeService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
// File/Location Services
builder.Services.AddSingleton<IFileProvider>(builder.Environment.WebRootFileProvider);
builder.Services.AddSingleton<ILocationService, JsonLocationService>();
builder.Services.AddScoped<Application.Interfaces.IEmailService, Infrastructure.Services.EmailService>();
builder.Services.AddScoped<Application.Interfaces.ITenant, Infrastructure.Repositories.TenantRepository>();
builder.Services.AddScoped<Application.Services.Tenants.ITenantService, Application.Services.Tenants.TenantService>();

// --- 2. BUILD THE APP ---
var app = builder.Build();

// --- 3. MIDDLEWARE PIPELINE ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseStaticFiles();

app.UseAuthentication(); // Must be before Authorization
app.UseAuthorization();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var devEmail = "guriza291@gmail.com";
    if (await userManager.FindByEmailAsync(devEmail) == null)
    {
        var person = new Domain.Entities.Person
        {
            FirstName = "Developer",
            LastName = "Admin",
            Email = devEmail,
            CreatedBy = "System",
            UpdateBy = "System",
            TenantType = "Individual",
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync();

        var user = new User 
        { 
            UserName = devEmail, 
            Email = devEmail, 
            FirstName = "Developer", 
            LastName = "Admin",
            EmailConfirmed = true,
            PersonId = person.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        // Using a temporary password that satisfies default requirements (requires digit, uppercase, etc.)
        await userManager.CreateAsync(user, "12345678"); 
    }
    else
    {
        var user = await userManager.FindByEmailAsync(devEmail);
        user.PasswordHash = userManager.PasswordHasher.HashPassword(user, "12345678");
        var result = await userManager.UpdateAsync(user);
        Console.WriteLine($"[Seeder] Forced password update result: {result.Succeeded}");
    }
}

app.Run();