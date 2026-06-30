using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=TERANOS-V01;Database=DigitalLoanPlatForm;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True",
                sqlOptions => sqlOptions.MigrationsAssembly("Infrastructure"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
