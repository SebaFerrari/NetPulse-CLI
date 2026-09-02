using NetPulse_CLI.Core.Interfaces;
using NetPulse_CLI.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetPulse_CLI.Core.Services
{
    public class JsonReportExporter : IReportExporter<ScanReport> , IReportExporter<PingReport>
    {
        public string Format => "json";

        public Task ExportAsync(ScanReport report, string path, CancellationToken ct = default)
            => WriteAsync(report, path, ct);

        public Task ExportAsync(PingReport report, string path, CancellationToken ct = default)
            => WriteAsync(report, path, ct);

        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        private static async Task WriteAsync<T>(T report, string path, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(report, Options);
            await File.WriteAllTextAsync(path, json, ct);
        }
    }
}
