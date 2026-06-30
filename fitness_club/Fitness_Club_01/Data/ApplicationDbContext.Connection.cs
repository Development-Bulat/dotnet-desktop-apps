using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Data;

public partial class ApplicationDbContext
{
    private const string DefaultConnection =
        "Host=localhost;Port=5432;Database=Fitness_Club;Username=YOUR_USER;Password=YOUR_PASSWORD";

    public ApplicationDbContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connection =
                Environment.GetEnvironmentVariable("FITNESS_CLUB_CONNECTION")
                ?? TryLoadLocalConnection()
                ?? DefaultConnection;

            optionsBuilder.UseNpgsql(connection);
        }
    }

    private static string? TryLoadLocalConnection()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "connection.local.txt");
        if (!File.Exists(path))
            return null;

        var line = File.ReadAllText(path).Trim();
        return string.IsNullOrWhiteSpace(line) ? null : line;
    }
}
