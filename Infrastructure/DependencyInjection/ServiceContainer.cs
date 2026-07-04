using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Data;
using System.Diagnostics;
using Infrastructure.Repositories;
using Application.Interfaces;
using Application.Services.Borrowers;
using Application.Services.BorrowerTypes;
using Application.Services.LoanApplications;
using Application.Services.ProvidedDocuments;
using Application.Services.Guarantors;
using Application.Services.GuarantorTypes;
using Application.Services.PaymentModalities;
using Application.Services.Accounts;
using Application.Services.LoanProducts;
using Application.Services.RequiredDocuments;
using Infrastructure.Identity;
using Application.Services.AccountTypes;
using Application.Services.Collaterals;
using Application.Services.Waivers;

namespace Infrastructure.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register ApplicationDbContext (service here) with SQL Server provider
            // services.AddDbContext<ApplicationDbContext>(options =>
            //  options.UseSqlServer(configuration.GetConnectionString("LoanPlatformDBCONN")),ServiceLifetime.Scoped
            //     );
                //Register authentication services
            // services.AddAuthenticationService(configuration);

             services.AddHttpContextAccessor();
             
             services.AddScoped<IBorrower, BorrowerRepository>();
             services.AddScoped<IBorrowerType, BorrowerTypeRepository>();
             services.AddScoped<ILoanApplication, LoanApplicationRepository>();
             services.AddScoped<IGuarantor, GuarantorRepository>();
             services.AddScoped<IGuarantorType, GuarantorTypeRepository>();
             services.AddScoped<IProvidedDocument, ProvidedDocumentRepository>();
             services.AddScoped<IPaymentModality, PaymentModalityRepository>();
              services.AddScoped<IIdentity, IdentityRepository>();
              services.AddScoped<IAccount, AccountRepository>();
            services.AddScoped<ILoanProduct, LoanProductRepository>();
            services.AddScoped<IRequiredDocument, RequiredDocumentRepository>();
            services.AddScoped<IRequirement, RequirementRepository>();
            services.AddScoped<IAccountType, AccountTypeRepository>();
             services.AddScoped<IPerson, PersonRepository>();
             services.AddScoped<IRequirement, RequirementRepository>();

              services.AddScoped<IDisbursement, DisbursementRepository>();
            services.AddScoped<IReason, ReasonRepository>();
            services.AddScoped<IPayment, PaymentRepository>();
            services.AddScoped<IPenality, PenalityRepository>();
            services.AddScoped<IPaymentType, PaymentTypeRepository>();
            services.AddScoped<ICollateral, CollateralRepository>();
            services.AddScoped<IProcessFeeDeposit, ProcessFeeDepositRepository>();
            services.AddScoped<ILoanProductSetting, LoanProductSettingRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();



             return services;
        }
    }
}