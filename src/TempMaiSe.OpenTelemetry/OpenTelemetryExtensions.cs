using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;

using System.Diagnostics.Metrics;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TempMaiSe.OpenTelemetry;

/// <summary>
/// Provides extension methods for configuring OpenTelemetry tracing and metrics in a <see cref="WebApplicationBuilder"/>.
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Adds OpenTelemetry tracing and metrics configuration to the <see cref="WebApplicationBuilder"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <param name="appBuilder">The <see cref="WebApplicationBuilder"/> to configure.</param>
    /// <summary>
    /// Adds OpenTelemetry tracing and metrics configuration to the <see cref="WebApplicationBuilder"/>.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <param name="appBuilder">The <see cref="WebApplicationBuilder"/> to configure.</param>
    public static void AddOpenTelemetry<TService>(this WebApplicationBuilder appBuilder)
    {
        ArgumentNullException.ThrowIfNull(appBuilder);

        // Note: Switch between Zipkin/Jaeger/OTLP/Console by setting UseTracingExporter in appsettings.json.
        string tracingExporter = appBuilder.Configuration.GetValue<string>("UseTracingExporter")?.ToLowerInvariant();

        // Note: Switch between Prometheus/OTLP/Console by setting UseMetricsExporter in appsettings.json.
        string metricsExporter = appBuilder.Configuration.GetValue<string>("UseMetricsExporter")?.ToLowerInvariant();

        // Note: Switch between Console/OTLP by setting UseLogExporter in appsettings.json.
        string logExporter = appBuilder.Configuration.GetValue<string>("UseLogExporter")?.ToLowerInvariant();

        // Note: Switch between Explicit/Exponential by setting HistogramAggregation in appsettings.json
        string histogramAggregation = appBuilder.Configuration.GetValue<string>("HistogramAggregation")?.ToLowerInvariant();

        // Build a resource configuration action to set service information.
        Action<ResourceBuilder> configureResource = r => r.AddService(
            serviceName: appBuilder.Configuration?.GetValue<string>("ServiceName") ?? "unknown",
            serviceVersion: typeof(TService).Assembly.GetName().Version?.ToString() ?? "unknown",
            serviceInstanceId: Environment.MachineName);

        // Create a service to expose ActivitySource, and Metric Instruments
        // for manual instrumentation
        appBuilder.Services.AddSingleton<IMailingInstrumentation, MailingInstrumentation>();

        // Configure OpenTelemetry tracing & metrics with auto-start using the
        // AddOpenTelemetry extension from OpenTelemetry.Extensions.Hosting.
        appBuilder.Services.AddOpenTelemetry()
            .ConfigureResource(configureResource)
            .WithTracing(builder =>
            {
                // Tracing

                // Ensure the TracerProvider subscribes to any custom ActivitySources.
                builder
                    .AddSource(MailingInstrumentation.ActivitySourceName)
                    .SetSampler(new AlwaysOnSampler())
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation();

                // Use IConfiguration binding for AspNetCore instrumentation options.
                appBuilder.Services.Configure<AspNetCoreTraceInstrumentationOptions>(appBuilder.Configuration.GetSection("AspNetCoreTraceInstrumentation"));

                switch (tracingExporter)
                {
                    case "zipkin":
                        builder.AddZipkinExporter();

                        builder.ConfigureServices(services =>
                        {
                            // Use IConfiguration binding for Zipkin exporter options.
                            services.Configure<ZipkinExporterOptions>(appBuilder.Configuration.GetSection("Zipkin"));
                        });
                        break;

                    case "otlp":
                        builder.AddOtlpExporter(otlpOptions =>
                        {
                            // Use IConfiguration directly for Otlp exporter endpoint option.
                            otlpOptions.Endpoint = new Uri(appBuilder.Configuration.GetValue<string>("Otlp:Endpoint"));
                        });
                        break;

                    default:
                        builder.AddConsoleExporter();
                        break;
                }
            })
            .WithMetrics(builder =>
            {
                // Metrics

                // Ensure the MeterProvider subscribes to any custom Meters.
                builder
                    .AddMeter(MailingInstrumentation.MeterName)
                    //.SetExemplarFilter(new TraceBasedExemplarFilter())
                    //.AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation();

                switch (histogramAggregation)
                {
                    case "exponential":
                        builder.AddView(instrument =>
                        {
                            return instrument.GetType().GetGenericTypeDefinition() == typeof(Histogram<>)
                                ? new Base2ExponentialBucketHistogramConfiguration()
                                : null;
                        });
                        break;
                    default:
                        // Explicit bounds histogram is the default.
                        // No additional configuration necessary.
                        break;
                }

                switch (metricsExporter)
                {
                    case "prometheus":
                        builder.AddPrometheusExporter();
                        break;
                    case "otlp":
                        builder.AddOtlpExporter(otlpOptions =>
                        {
                            // Use IConfiguration directly for Otlp exporter endpoint option.
                            otlpOptions.Endpoint = new Uri(appBuilder.Configuration.GetValue<string>("Otlp:Endpoint")!);
                        });
                        break;
                    default:
                        builder.AddConsoleExporter();
                        break;
                }
            });

        // Clear default logging providers used by WebApplication host.
        appBuilder.Logging.ClearProviders();

        // Configure OpenTelemetry Logging.
        appBuilder.Logging.AddOpenTelemetry(options =>
        {
            // Note: See appsettings.json Logging:OpenTelemetry section for configuration.

            var resourceBuilder = ResourceBuilder.CreateDefault();
            configureResource(resourceBuilder);
            options.SetResourceBuilder(resourceBuilder);

            switch (logExporter)
            {
                case "otlp":
                    options?.AddOtlpExporter(otlpOptions =>
                    {
                        // Use IConfiguration directly for Otlp exporter endpoint option.
                        otlpOptions.Endpoint = new Uri(appBuilder.Configuration.GetValue<string>("Otlp:Endpoint"));
                    });
                    break;
                default:
                    options.AddConsoleExporter();
                    break;
            }
        });
    }
}
