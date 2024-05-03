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
    /// <summary>
    /// Adds the mail service to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the mail service to.</param>
    /// <param name="configureParser">An optional action to configure the <see cref="FluidParser"/>.</param>
    public static void AddMailService(this IServiceCollection services, Action<IServiceProvider, FluidParser>? configureParser = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(serviceProvider =>
        {
            FluidParser parser = new();
            parser.RegisteredOperators["has_inline_image"] = (a, b) => new HasInlineImageBinaryExpression(a, b);
            parser.RegisterExpressionTag("inline_image", InlineImageTag.WriteToAsync);
            configureParser?.Invoke(serviceProvider, parser);
            return parser;
        });

        services.TryAddSingleton<ITemplateToMailMapper, TemplateToMailMapper>();
        services.TryAddSingleton<IMailInformationToMailMapper, MailInformationToMailMapper>();

        services.TryAddSingleton<IDataParser, DataParser>();

        services.TryAddScoped<IMailService, MailService>();
    }
}
