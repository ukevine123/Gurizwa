using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Infrastructure.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Try to find the Web project directory to load appsettings.json
            var basePath = Directory.GetCurrentDirectory();
            
            // If running from Infrastructure directory or solution directory, adjust path to find Web
            if (!File.Exists(Path.Combine(basePath, "appsettings.json")) && 
                !File.Exists(Path.Combine(basePath, "appsettings.Development.json")))
            {
                var webPath = Path.Combine(basePath, "Web");
                if (Directory.Exists(webPath))
                {
                    basePath = webPath;
                }
                else
                {
                    // Maybe we are in Infrastructure folder
                    var parentDir = Directory.GetParent(basePath)?.FullName;
                    if (parentDir != null)
                    {
                        var siblingWebPath = Path.Combine(parentDir, "Web");
                        if (Directory.Exists(siblingWebPath))
                        {
                            basePath = siblingWebPath;
                        }
                    }
                }
            }

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables();

            var configuration = configBuilder.Build();
            var connectionString = configuration.GetConnectionString("LoanPlatformDBCONN");

            if (string.IsNullOrEmpty(connectionString))
            {
                // Fallback to localdb if connection string not found
                connectionString = "Server=(localdb)\\mssqllocaldb;Database=DigitalLoanPlatform;Trusted_Connection=True;MultipleActiveResultSets=true";
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString, sqlOptions => {
                sqlOptions.MigrationsAssembly("Infrastructure");
                sqlOptions.UseCompatibilityLevel(120);
            });

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
