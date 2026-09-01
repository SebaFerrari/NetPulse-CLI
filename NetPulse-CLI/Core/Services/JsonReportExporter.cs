using NetPulse_CLI.Core.Interfaces;
using NetPulse_CLI.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetPulse_CLI.Core.Services
{
    public class JsonReportExporter : IReportExporter
    {
        public string Format => "json";

        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        public async Task ExportAsync(ScanReport report, string path, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(report, Options);
            await File.WriteAllTextAsync(path, json, ct);
        }
    }
}
