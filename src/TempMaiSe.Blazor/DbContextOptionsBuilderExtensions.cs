using Microsoft.EntityFrameworkCore;
using TempMaiSe.Models;

namespace TempMaiSe.Blazor;

public static class DbContextOptionsBuilderExtensions
{
    public static IServiceCollection AddMailingContext(this IServiceCollection services, ConfigurationManager config)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(config);

        return services.AddDbContext<MailingContext>(options =>
        {
            string? provider = config.GetValue("provider", Provider.InMemory.Name);
            if (string.IsNullOrWhiteSpace(provider) || provider == Provider.InMemory.Name)
            {
                options.UseInMemoryDatabase(nameof(TempMaiSe));
                return;
            }

            if (provider == Provider.Sqlite.Name)
            {
                options.UseSqlite(
                    config.GetConnectionString(Provider.Sqlite.Name)!,
                    x => x.MigrationsAssembly(Provider.Sqlite.Assembly)
                );
                return;
            }

            if (provider == Provider.SqlServer.Name)
            {
                options.UseNpgsql(
                    config.GetConnectionString(Provider.SqlServer.Name)!,
                    x => x.MigrationsAssembly(Provider.SqlServer.Assembly)
                );
                return;
            }

            if (provider == Provider.PostgreSql.Name)
            {
                options.UseNpgsql(
                    config.GetConnectionString(Provider.PostgreSql.Name)!,
                    x => x.MigrationsAssembly(Provider.PostgreSql.Assembly)
                );
                return;
            }
        });
    }
}
