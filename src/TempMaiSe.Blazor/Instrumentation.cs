using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace TempMaiSe.Blazor;

/// <summary>
/// It is recommended to use a custom type to hold references for
/// ActivitySource and Instruments. This avoids possible type collisions
/// with other components in the DI container.
/// </summary>
public sealed class Instrumentation : IDisposable
{
    internal const string ActivitySourceName = "TempMaiSe.Blazor";
    internal const string MeterName = "TempMaiSe.Blazor";
    private readonly Meter _meter;

    public Instrumentation()
    {
        string? version = typeof(Instrumentation).Assembly.GetName().Version?.ToString();
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
