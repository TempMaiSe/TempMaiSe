using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace TempMaiSe.OpenTelemetry;

/// <summary>
/// It is recommended to use a custom type to hold references for
/// ActivitySource and Instruments. This avoids possible type collisions
/// with other components in the DI container.
/// </summary>
public sealed class MailingInstrumentation : IMailingInstrumentation, IDisposable
{
    internal const string ActivitySourceName = "TempMaiSe";

    internal const string MeterName = "TempMaiSe";

    private readonly Meter _meter;

    public MailingInstrumentation()
    {
        string? version = typeof(MailingInstrumentation).Assembly.GetName().Version?.ToString();
        ActivitySource = new ActivitySource(ActivitySourceName, version);
        _meter = new Meter(MeterName, version);
        MailsSent = _meter.CreateCounter<long>("mail.sent.count", "E-Mails sent");
    }

    public ActivitySource ActivitySource { get; }

    public Counter<long> MailsSent { get; }

    public void Dispose()
    {
        ActivitySource.Dispose();
        _meter.Dispose();
    }
}