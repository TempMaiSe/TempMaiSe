using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace TempMaiSe.OpenTelemetry;

public interface IMailingInstrumentation
{
    ActivitySource ActivitySource { get; }

    Counter<long> MailsSent { get; }
}
