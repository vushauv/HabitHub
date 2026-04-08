using backend.Configuration;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace backend.Data;

// Used only by `dotnet ef` CLI — not registered in DI
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        Env.TraversePath().Load();

        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables(prefix: "BACKEND__")
            .Build();

        var settings = configuration.Get<AppSettings>()
            ?? throw new InvalidOperationException("Missing required BACKEND__ environment variables.");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(settings.ConnectionString)
            .Options;

        return new AppDbContext(options);
    }
}
