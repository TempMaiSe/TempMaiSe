using FluentEmail.Mailgun;
using FluentEmail.MailKitSmtp;

namespace TempMaiSe.Razor;

public static class FluentEmailBuilderExtensions
{
    private const string MailKit = nameof(MailKit);
    private const string MailGun = nameof(MailGun);
    private const string Mailtrap = nameof(Mailtrap);
    private const string SendGrid = nameof(SendGrid);

    private static readonly HashSet<string> s_supportedSenders = new(4) { MailKit, MailGun, Mailtrap, SendGrid };

    public static FluentEmailServicesBuilder AddFluentEmail(this IServiceCollection services, ConfigurationManager config)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(config);

        IConfigurationSection fluentEmailConfigSection = config.GetRequiredSection(nameof(FluentEmail));
        return services.AddFluentEmail(
            fluentEmailConfigSection.GetValue("FromAddress", "dummy@example.org"),
            fluentEmailConfigSection.GetValue("FromName", ""))
            .AddSenderFromConfiguration(fluentEmailConfigSection);
    }

    private static FluentEmailServicesBuilder AddSenderFromConfiguration(this FluentEmailServicesBuilder builder, IConfigurationSection fluentEmailConfigSection)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(fluentEmailConfigSection);

        string? sender = fluentEmailConfigSection.GetValue("Sender", "None");
        if (sender == null || !s_supportedSenders.Contains(sender))
        {
            throw new InvalidOperationException($"Sender '{sender}' is not valid.");
        }

        switch (sender)
        {
            case MailKit:
                SmtpClientOptions mailKitOptions = fluentEmailConfigSection.GetRequiredSection(MailKit).Get<SmtpClientOptions>()
                    ?? throw new InvalidOperationException($"Sender '{sender}' requires a valid config section.");
                builder = builder.AddMailKitSender(mailKitOptions);
                break;
            case MailGun:
                IConfigurationSection mailGunConfigSection = fluentEmailConfigSection.GetRequiredSection(MailGun);
                string? domainName = mailGunConfigSection.GetValue("ServerName", "example.org")!;
                string? apiKey = mailGunConfigSection.GetValue("ApiKey", "")!;
                MailGunRegion region = mailGunConfigSection.GetValue("Region", MailGunRegion.USA);
                builder = builder.AddMailGunSender(domainName, apiKey, region);
                break;
            case Mailtrap:
                IConfigurationSection mailtrapConfigSection = fluentEmailConfigSection.GetRequiredSection(Mailtrap);
                string? userName = mailtrapConfigSection.GetValue("UserName", "")!;
                string? password = mailtrapConfigSection.GetValue("Password", "")!;
                builder = builder.AddMailtrapSender(userName, password);
                break;
            case SendGrid:
                IConfigurationSection sendGridConfigSection = fluentEmailConfigSection.GetRequiredSection(SendGrid);
                string? sendGridApiKey = sendGridConfigSection.GetValue("ApiKey", "")!;
                bool sandBoxMode = sendGridConfigSection.GetValue("SandBoxMode", false)!;
                builder = builder.AddSendGridSender(sendGridApiKey, sandBoxMode);
                break;
            default:
                // Just don't configure a sender. Maybe log a warning about this?
                break;
        }

        return builder;
    }
}
