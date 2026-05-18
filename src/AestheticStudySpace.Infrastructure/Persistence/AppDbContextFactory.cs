using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AestheticStudySpace.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations (dotnet ef).
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../AestheticStudySpace.Api");
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Database=AestheticStudySpace;Trusted_Connection=True;TrustServerCertificate=True;";

        var provider = configuration["Database:Provider"] ?? "SqlServer";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        if (string.Equals(provider, "PostgreSQL", StringComparison.OrdinalIgnoreCase))
            optionsBuilder.UseNpgsql(connectionString);
        else
            optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
