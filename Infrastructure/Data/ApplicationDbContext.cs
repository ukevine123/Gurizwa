using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Identity;
using System.Linq;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<Borrower> Borrowers { get; set; }
        public DbSet<Guarantor> Guarantors { get; set; }
        public DbSet<GuarantorType> GuarantorTypes { get; set; }
        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<PaymentModality> PaymentModalities { get; set; }
        public DbSet<ProvidedDocument> ProvidedDocuments { get; set; }
        public DbSet<BorrowerType> BorrowerTypes { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<LoanProduct> LoanProducts { get; set; }
        public DbSet<RequiredDocument> RequiredDocuments { get; set; }
        public DbSet<Requirement> Requirements { get; set; }
        public DbSet<AccountType> AccountTypes { get; set; }
        public DbSet<Disbursement> Disbursements { get; set; } // Removed duplicate
        public DbSet<Person> Persons { get; set; }
        public DbSet<Reason> Reasons { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentType> PaymentTypes { get; set; }
        public DbSet<Penality> Penalties { get; set; }
        public DbSet<Collateral> Collaterals { get; set; }
        public DbSet<ProcessFeeDeposit> ProcessFeeDeposits { get; set; }
        public DbSet<LoanProductSetting> LoanProductSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // 1. MUST call base first for Identity configurations
            base.OnModelCreating(builder);

            // 2. Fix Cascade Path for Disbursements
            builder.Entity<Disbursement>()
                .HasOne(d => d.PaymentModality)
                .WithMany() 
                .HasForeignKey(d => d.PaymentModalityId)
                .OnDelete(DeleteBehavior.Restrict);
    // Disable cascade delete between Borrower and ProcessFeeDeposits
        builder.Entity<ProcessFeeDeposit>()
        .HasOne(p => p.Borrower)
        .WithMany() 
        .HasForeignKey(p => p.BorrowerId)
        .OnDelete(DeleteBehavior.NoAction);
            // 3. Set Decimal Precision globally
            foreach (var property in builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }

            // 4. Enum Conversions
            builder.Entity<LoanApplication>()
                .Property(t => t.Status)
                .HasConversion<string>();

            // 5. Identity Table Renaming
            builder.Entity<User>().ToTable("Users");
            builder.Entity<IdentityRole<int>>().ToTable("Roles");
            builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
            

            // Note: base.OnModelCreating already handles the composite key for UserRoles. 
            // Re-declaring it is usually unnecessary unless you've changed the Identity behavior significantly.
        }
    }
}