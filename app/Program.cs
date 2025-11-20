using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Net48OtelSignozDemo
{
    internal class Program
    {
        // ActivitySource for custom spans
        private static readonly ActivitySource ActivitySource =
            new ActivitySource("Net48OtelSignozDemo");

        private const string ServiceName = "net48-signoz-demo";
        private const string ServiceVersion = "1.0.0";

        // Local collector endpoint (Docker)
        private const string CollectorOtlpEndpoint = "http://localhost:4317";

        private static async Task Main(string[] args)
        {
            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(ServiceName, serviceVersion: ServiceVersion)
                .AddAttributes(new[]
                {
                    new KeyValuePair<string, object>("deployment.environment", "staging")
                });

            var collectorUri = new Uri(CollectorOtlpEndpoint);

            // ---- TRACES ----
            var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddSource("Net48OtelSignozDemo")          // manual spans
                .AddHttpClientInstrumentation()           // outgoing HTTP spans
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = collectorUri;
                    o.Protocol = OtlpExportProtocol.Grpc; // talk to collector's gRPC receiver
                    // No headers needed; collector forwards to SigNoz
                })
                .Build();

            // ---- METRICS ----
            var meterProvider = Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddRuntimeInstrumentation()              // GC, CPU, etc.
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = collectorUri;
                    o.Protocol = OtlpExportProtocol.Grpc;
                })
                .Build();

            // ---- LOGGING ----
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                // Console logs (for you)
                builder.AddConsole();

                // OpenTelemetry logs (to collector)
                builder.AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(resourceBuilder);
                    options.IncludeScopes = true;
                    options.IncludeFormattedMessage = true;

                    options.AddOtlpExporter(o =>
                    {
                        o.Endpoint = collectorUri;
                        o.Protocol = OtlpExportProtocol.Grpc;
                    });
                });
            });

            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogInformation("Starting Net48OtelSignozDemo...");

            // Generate some telemetry
            for (int i = 0; i < 5; i++)
            {
                await RunDemoIterationAsync(logger, i);
                await Task.Delay(1000);
            }

            logger.LogInformation("Demo finished, flushing providers...");

            tracerProvider?.ForceFlush();
            meterProvider?.ForceFlush();

            tracerProvider?.Dispose();
            meterProvider?.Dispose();

            Console.WriteLine("Press ENTER to exit.");
            Console.ReadLine();
        }

        private static async Task RunDemoIterationAsync(ILogger logger, int iteration)
        {
            using (var activity = ActivitySource.StartActivity("demo-operation"))
            {
                if (activity != null)
                {
                    activity.SetTag("demo.iteration", iteration);
                    activity.SetTag("demo.tag", "value");
                    activity.SetTag("operation.id", Guid.NewGuid().ToString("N"));
                }

                logger.LogInformation("Iteration {Iteration}: inside demo-operation span.", iteration);

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync("https://example.com/");
                    logger.LogInformation(
                        "Iteration {Iteration}: HTTP GET to example.com returned {StatusCode}",
                        iteration,
                        response.StatusCode);
                }

                logger.LogWarning("Iteration {Iteration}: finishing demo-operation.", iteration);
            }
        }
    }
}