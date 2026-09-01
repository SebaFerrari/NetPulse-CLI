using NetPulse_CLI.Core.Interfaces;
using NetPulse_CLI.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetPulse_CLI.Core.Services
{
    public class CsvReportExporter : IReportExporter
    {
        public string Format => "csv";

        public async Task ExportAsync(ScanReport report, string path, CancellationToken ct = default)
        {
            var sb = new StringBuilder();
            sb.AppendLine("host,port,status,timestamp");

            foreach (var r in report.Results)
            {
                sb.Append(Escape(r.Host)).Append(',')
                  .Append(r.Port).Append(',')
                  .Append(r.Status).Append(',')
                  .Append(r.Timestamp.ToString("o", CultureInfo.InvariantCulture))
                  .AppendLine();
            }

            await File.WriteAllTextAsync(path, sb.ToString(), ct);
        }

        private static string Escape(string value)
        {
            if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n'))
                return value;

            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
    }
}
