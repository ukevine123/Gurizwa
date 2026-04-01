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


var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVICES REGISTRATION ---

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("LoanPlatformDBCONN");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, x => x.MigrationsAssembly("Infrastructure")));

builder.Services.AddScoped(p => 
    p.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

// Identity Configuration (ONLY ONE REGISTRATION)
// Note: Ensure your 'User' class in Infrastructure matches IdentityRole<int> if that's your preference
builder.Services.AddIdentity<User, IdentityRole<int>>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Core Services
builder.Services.AddScoped<UserContext>();
builder.Services.AddMudServices();
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
// File/Location Services
builder.Services.AddSingleton<IFileProvider>(builder.Environment.WebRootFileProvider);
builder.Services.AddSingleton<ILocationService, JsonLocationService>();

// --- 2. BUILD THE APP ---
var app = builder.Build();

// --- 3. MIDDLEWARE PIPELINE ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.UseAuthentication(); // Must be before Authorization
app.UseAuthorization();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapControllers();

app.Run();