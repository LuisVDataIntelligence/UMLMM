using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UMLMM.Infrastructure.Data;

public class UmlmmDbContextFactory : IDesignTimeDbContextFactory<UmlmmDbContext>
{
    public UmlmmDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UmlmmDbContext>();

        // Use a default connection string for migrations
        // In production, this should come from configuration
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Database=umlmm;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new UmlmmDbContext(optionsBuilder.Options);
    }
}
