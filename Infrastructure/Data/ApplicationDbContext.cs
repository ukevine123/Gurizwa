using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Identity;
using System.Linq;
using Application.Interfaces;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        private readonly IUserContext? _userContext;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IUserContext? userContext = null)
            : base(options)
        {
            _userContext = userContext;
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
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<Waiver> Waivers { get; set; }
        public DbSet<WaiverType> WaiverTypes { get; set; }
        public DbSet<Expense> Expenses { get; set; }

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

            builder.Entity<Disbursement>()
                .HasOne(d => d.Account)
                .WithMany()
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Disbursement>()
                .HasOne(d => d.LoanApplication)
                .WithMany()
                .HasForeignKey(d => d.LoanApplicationId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ProcessFeeDeposit>()
                .HasOne(pfd => pfd.LoanApplication)
                .WithMany()
                .HasForeignKey(pfd => pfd.LoanApplicationId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ProcessFeeDeposit>()
                .HasOne(pfd => pfd.Account)
                .WithMany()
                .HasForeignKey(pfd => pfd.AccountId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Payment>()
                .HasOne(p => p.Disbursement)
                .WithMany(d => d.Payments)
                .HasForeignKey(p => p.DisbursementId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Payment>()
                .HasOne(p => p.Account)
                .WithMany()
                .HasForeignKey(p => p.AccountId)
                .OnDelete(DeleteBehavior.NoAction);
            
            // Fix Cascade Paths involving Person entity to prevent multiple cascade delete paths
            builder.Entity<LoanApplication>()
                .HasOne(la => la.Person)
                .WithMany(p => p.LoanApplications)
                .HasForeignKey(la => la.PersonId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<LoanProductSetting>()
                .HasOne(lps => lps.Person)
                .WithMany(p => p.LoanProductSettings)
                .HasForeignKey(lps => lps.PersonId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<LoanProduct>()
                .HasOne(lp => lp.Person)
                .WithMany(p => p.LoanProducts)
                .HasForeignKey(lp => lp.PersonId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Collateral>()
                .HasOne(c => c.Person)
                .WithMany(p => p.Collaterals)
                .HasForeignKey(c => c.PersonId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Guarantor>()
                .HasOne(g => g.Person)
                .WithMany(p => p.Guarantors)
                .HasForeignKey(g => g.PersonId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Guarantor>()
                .HasOne(g => g.LoanApplication)
                .WithMany()
                .HasForeignKey(g => g.LoanApplicationId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Account>()
                .HasOne(a => a.Person)
                .WithMany(p => p.Accounts)
                .HasForeignKey(a => a.PersonId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ProvidedDocument>()
                .HasOne(pd => pd.Person)
                .WithMany(p => p.ProvidedDocuments)
                .HasForeignKey(pd => pd.PersonId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<RequiredDocument>()
                .HasOne(rd => rd.Person)
                .WithMany(p => p.RequiredDocuments)
                .HasForeignKey(rd => rd.PersonId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Requirement>()
                .HasOne(r => r.Person)
                .WithMany(p => p.Requirements)
                .HasForeignKey(r => r.PersonId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Borrower>()
                .HasOne(b => b.Person)
                .WithMany(p => p.Borrowers)
                .HasForeignKey(b => b.PersonId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Penality>()
                .HasOne(p => p.Person)
                .WithMany(p => p.Penalities)
                .HasForeignKey(p => p.PersonId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Penality>()
                .HasOne(p => p.Reason)
                .WithMany()
                .HasForeignKey(p => p.ReasonId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Payment>()
                .HasOne(p => p.Person)
                .WithMany(p => p.Payments)
                .HasForeignKey(p => p.PersonId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Expense>()
                .HasOne(e => e.Person)
                .WithMany()
                .HasForeignKey(e => e.PersonId)
                .OnDelete(DeleteBehavior.NoAction);
    // Disable cascade delete between Borrower and ProcessFeeDeposits
       
            // 3. Set Decimal Precision globally

            foreach (var property in builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }

            builder.Entity<ActivityLog>(entity =>
            {
                entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
                entity.Property(e => e.EntityName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.EntityId).HasMaxLength(64).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.UserId).HasMaxLength(64).IsRequired();
                entity.Property(e => e.UserName).HasMaxLength(200).IsRequired();
                entity.HasIndex(e => e.Timestamp);
            });

            // 4. Enum Conversions
            builder.Entity<LoanApplication>()
                .Property(t => t.Status)
                .HasConversion<string>();

            builder.Entity<LoanApplication>()
                .HasOne(la => la.PaymentModality)
                .WithMany()
                .HasForeignKey(la => la.PaymentModalityId)
                .OnDelete(DeleteBehavior.NoAction);

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

        public override int SaveChanges()
        {
            ApplyAuditInformation();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplyAuditInformation();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInformation();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ApplyAuditInformation();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void ApplyAuditInformation()
        {
            var now = DateTime.Now;
            var userName = GetCurrentUserName();
            var userId = _userContext?.Id;
            var hasCurrentUser = !string.IsNullOrWhiteSpace(userName);

            foreach (var entry in ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                if (entry.Entity is ActivityLog)
                {
                    continue;
                }

                if (entry.State == EntityState.Added)
                {
                    SetDateProperty(entry, "CreatedAt", now);
                    SetUserProperty(entry, "CreatedBy", userName, userId, hasCurrentUser);

                    SetDateProperty(entry, "UpdatedAt", now);
                    SetUserProperty(entry, "UpdatedBy", userName, userId, hasCurrentUser);
                    SetUserProperty(entry, "UpdateBy", userName, userId, hasCurrentUser);
                }
                else if (entry.State == EntityState.Modified)
                {
                    SetDateProperty(entry, "UpdatedAt", now);
                    SetUserProperty(entry, "UpdatedBy", userName, userId, hasCurrentUser);
                    SetUserProperty(entry, "UpdateBy", userName, userId, hasCurrentUser);
                }
            }
        }

        private string GetCurrentUserName()
        {
            if (!string.IsNullOrWhiteSpace(_userContext?.FullName))
            {
                return _userContext.FullName;
            }

            if (!string.IsNullOrWhiteSpace(_userContext?.Email))
            {
                return _userContext.Email;
            }

            return string.Empty;
        }

        private static void SetDateProperty(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, string propertyName, DateTime value)
        {
            var property = entry.Metadata.FindProperty(propertyName);
            if (property == null)
            {
                return;
            }

            if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
            {
                entry.Property(propertyName).CurrentValue = value;
            }
        }

        private static void SetUserProperty(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, string propertyName, string userName, int? userId, bool hasCurrentUser)
        {
            var property = entry.Metadata.FindProperty(propertyName);
            if (property == null)
            {
                return;
            }

            var entryProperty = entry.Property(propertyName);

            if (property.ClrType == typeof(string))
            {
                if (!hasCurrentUser && !string.IsNullOrWhiteSpace(entryProperty.CurrentValue as string))
                {
                    return;
                }

                entryProperty.CurrentValue = hasCurrentUser ? userName : "System";
                return;
            }

            if ((property.ClrType == typeof(int) || property.ClrType == typeof(int?)) && userId.HasValue)
            {
                entryProperty.CurrentValue = userId.Value;
            }
        }
    }
}
