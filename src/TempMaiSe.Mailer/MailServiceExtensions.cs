using Fluid;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TempMaiSe.Mailer;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for adding the mail service to the <see cref="IServiceCollection"/>.
/// </summary>
public static class MailServiceExtensions
{
    /// <summary>
    /// Adds the mail service to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the mail service to.</param>
    public static void AddMailService(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<FluidParser>();

        services.TryAddSingleton<ITemplateToMailHeadersMapper, TemplateToMailHeadersMapper>();
        services.TryAddSingleton<IMailInformationToMailHeadersMapper, MailInformationToMailHeadersMapper>();

        services.TryAddSingleton<IDataParser, DataParser>();

        services.TryAddScoped<IMailService, MailService>();
    }
}
