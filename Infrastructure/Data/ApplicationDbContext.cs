using Microsoft.EntityFrameworkCore;
using Domain.Entities;


namespace Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
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
           public DbSet<Disbursement> Disbursements{get;set;}
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
           
        }
    }
}