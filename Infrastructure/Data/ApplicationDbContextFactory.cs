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
                "Server=(localdb)\\mssqllocaldb;Database=DigitalLoanPlatform;Trusted_Connection=True;MultipleActiveResultSets=true",
                sqlOptions => sqlOptions.MigrationsAssembly("Infrastructure"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
