using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UMLMM.Infrastructure.Persistence;

namespace UMLMM.DbTool;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? "Host=localhost;Database=umlmm;Username=postgres;Password=postgres";

        Console.WriteLine("UMLMM Database Tool");
        Console.WriteLine("===================");
        Console.WriteLine();

        if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
        {
            PrintHelp();
            return 0;
        }

        var command = args[0].ToLowerInvariant();

        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            await using var context = new AppDbContext(optionsBuilder.Options);

            switch (command)
            {
                case "migrate":
                case "apply":
                    Console.WriteLine($"Applying migrations to: {MaskConnectionString(connectionString)}");
                    await context.Database.MigrateAsync();
                    Console.WriteLine("✓ Migrations applied successfully!");
                    return 0;

                case "check":
                case "status":
                    Console.WriteLine($"Checking database status: {MaskConnectionString(connectionString)}");
                    var pending = await context.Database.GetPendingMigrationsAsync();
                    var applied = await context.Database.GetAppliedMigrationsAsync();
                    
                    Console.WriteLine($"Applied migrations: {applied.Count()}");
                    foreach (var migration in applied)
                    {
                        Console.WriteLine($"  ✓ {migration}");
                    }
                    
                    Console.WriteLine($"Pending migrations: {pending.Count()}");
                    foreach (var migration in pending)
                    {
                        Console.WriteLine($"  - {migration}");
                    }
                    
                    return pending.Any() ? 1 : 0;

                case "ensure":
                    Console.WriteLine($"Ensuring database exists: {MaskConnectionString(connectionString)}");
                    var created = await context.Database.EnsureCreatedAsync();
                    if (created)
                    {
                        Console.WriteLine("✓ Database created!");
                    }
                    else
                    {
                        Console.WriteLine("✓ Database already exists");
                    }
                    return 0;

                case "drop":
                    Console.WriteLine($"WARNING: This will drop the database: {MaskConnectionString(connectionString)}");
                    Console.Write("Type 'yes' to confirm: ");
                    var confirm = Console.ReadLine();
                    if (confirm?.ToLowerInvariant() == "yes")
                    {
                        await context.Database.EnsureDeletedAsync();
                        Console.WriteLine("✓ Database dropped");
                        return 0;
                    }
                    Console.WriteLine("Operation cancelled");
                    return 1;

                default:
                    Console.WriteLine($"Unknown command: {command}");
                    PrintHelp();
                    return 1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (args.Contains("--verbose") || args.Contains("-v"))
            {
                Console.WriteLine();
                Console.WriteLine("Stack trace:");
                Console.WriteLine(ex.StackTrace);
            }
            return 1;
        }
    }

    static void PrintHelp()
    {
        Console.WriteLine("Usage: dbtool [command] [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  migrate, apply    Apply pending migrations");
        Console.WriteLine("  check, status     Check migration status");
        Console.WriteLine("  ensure            Ensure database exists");
        Console.WriteLine("  drop              Drop the database (requires confirmation)");
        Console.WriteLine("  --help, -h        Show this help");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --verbose, -v     Show detailed error information");
        Console.WriteLine();
        Console.WriteLine("Connection String:");
        Console.WriteLine("  Set via appsettings.json, or DATABASE_URL environment variable");
        Console.WriteLine("  Default: Host=localhost;Database=umlmm;Username=postgres;Password=postgres");
    }

    static string MaskConnectionString(string connectionString)
    {
        // Simple masking for display purposes
        var parts = connectionString.Split(';');
        var masked = parts.Select(part =>
        {
            if (part.Contains("Password", StringComparison.OrdinalIgnoreCase))
            {
                return "Password=***";
            }
            return part;
        });
        return string.Join(";", masked);
    }
}
