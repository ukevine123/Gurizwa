using Web.Components;
using MudBlazor.Services;
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


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LoanPlatformDBCONN")));


    builder.Services.AddScoped(p => 
    p.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

// Add services to the container.
builder.Services.AddMudServices();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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

    ///Address case loading
    builder.Services.AddSingleton<IFileProvider>(builder.Environment.WebRootFileProvider);
  builder.Services.AddSingleton<ILocationService, JsonLocationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
