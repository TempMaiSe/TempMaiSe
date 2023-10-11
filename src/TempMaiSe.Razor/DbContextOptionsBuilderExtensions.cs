using Microsoft.EntityFrameworkCore;
using TempMaiSe.Models;

namespace TempMaiSe.Razor;

public static class DbContextOptionsBuilderExtensions
{
    public static IServiceCollection AddMailingContext(this IServiceCollection services, ConfigurationManager config)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(config);

        return services.AddDbContext<MailingContext>(options =>
        {
            string? provider = config.GetValue("provider", DbProvider.InMemory.Name);
            if (string.IsNullOrWhiteSpace(provider) || provider == DbProvider.InMemory.Name)
            {
                options.UseInMemoryDatabase(nameof(TempMaiSe));
                return;
            }

            if (provider == DbProvider.Sqlite.Name)
            {
                options.UseSqlite(
                    config.GetConnectionString(DbProvider.Sqlite.Name)!,
                    x => x.MigrationsAssembly(DbProvider.Sqlite.Assembly)
                );
                return;
            }

            if (provider == DbProvider.SqlServer.Name)
            {
                options.UseNpgsql(
                    config.GetConnectionString(DbProvider.SqlServer.Name)!,
                    x => x.MigrationsAssembly(DbProvider.SqlServer.Assembly)
                );
                return;
            }

            if (provider == DbProvider.PostgreSql.Name)
            {
                options.UseNpgsql(
                    config.GetConnectionString(DbProvider.PostgreSql.Name)!,
                    x => x.MigrationsAssembly(DbProvider.PostgreSql.Assembly)
                );
                return;
            }
        });
    }
}
