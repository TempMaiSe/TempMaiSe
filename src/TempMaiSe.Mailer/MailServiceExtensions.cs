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

        services.TryAddSingleton(_ =>
        {
            FluidParser parser = new();
            parser.RegisteredOperators["has_inline_image"] = (a, b) => new HasInlineImageBinaryExpression(a, b);
            parser.RegisterExpressionTag("inline_image", InlineImageTag.WriteToAsync);
            return parser;
        });

        services.TryAddSingleton<ITemplateToMailMapper, TemplateToMailMapper>();
        services.TryAddSingleton<IMailInformationToMailMapper, MailInformationToMailMapper>();

        services.TryAddSingleton<IDataParser, DataParser>();

        services.TryAddScoped<IMailService, MailService>();
    }
}
