using NetPulse_CLI.Core.Interfaces;
using NetPulse_CLI.Core.Models;
using NetPulse_CLI.Settings;
using NetPulse_CLI.UI;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics;

namespace NetPulse_CLI.Commands
{
    public sealed class ScanCommand : AsyncCommand<ScanSettings>
    {
        private readonly IPortScanner _scanner;
        private readonly IEnumerable<IReportExporter<ScanReport>> _exporters;

        public ScanCommand(IPortScanner scanner, IEnumerable<IReportExporter<ScanReport>> exporters)
            => (_scanner, _exporters) = (scanner, exporters);

        protected override async Task<int> ExecuteAsync(
            CommandContext context, ScanSettings settings, CancellationToken ct)
        {
            var total = settings.ToPort - settings.FromPort + 1;
            IReadOnlyList<ScanResult> results = [];

            var startedAt = DateTime.Now;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                results = await ProgressDisplay.RunWithProgressAsync(
                    "[green]Scanning ports[/]", total,
                    progress => _scanner.ScanRangeAsync(
                        settings.Host, settings.FromPort, settings.ToPort,
                        settings.TimeoutMs, settings.Concurrency, progress, ct));
            }
            catch (OperationCanceledException)
            {
                LiveTables.ShowCancelled("Scan");
                return 1;
            }

            stopwatch.Stop();

            LiveTables.ShowOpenPorts(results.Where(r => r.Status == PortStatus.Open).ToList());
            LiveTables.ShowScanSummary(settings.Host, results, stopwatch.Elapsed);

            if (!string.IsNullOrWhiteSpace(settings.OutputPath))
                return await ExportAsync(settings, results, startedAt, stopwatch.Elapsed, ct);

            return 0;
        }

        private async Task<int> ExportAsync(
            ScanSettings settings, IReadOnlyList<ScanResult> results,
            DateTime startedAt, TimeSpan duration, CancellationToken ct)
        {
            var extension = Path.GetExtension(settings.OutputPath!).TrimStart('.').ToLowerInvariant();
            var exporter = _exporters.FirstOrDefault(e => e.Format == extension);

            if (exporter is null)
            {
                LiveTables.ShowError(
                    $"Unsupported format '{extension}'. Available: {string.Join(", ", _exporters.Select(e => e.Format))}");
                return 1;
            }

            var report = new ScanReport(
                settings.Host, settings.FromPort, settings.ToPort, startedAt, duration, results);

            var fullPath = Path.GetFullPath(settings.OutputPath!);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            await exporter.ExportAsync(report, fullPath, ct);
            LiveTables.ShowReportSaved(fullPath);

            return 0;
        }
    }   
}
