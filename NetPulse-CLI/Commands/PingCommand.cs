using NetPulse_CLI.Core.Interfaces;
using NetPulse_CLI.Core.Models;
using NetPulse_CLI.Settings;
using NetPulse_CLI.UI;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics;
namespace NetPulse_CLI.Commands
{
    public sealed class PingCommand : AsyncCommand<PingSettings>
    {
        private readonly IPingService _pingService;
        private readonly IEnumerable<IReportExporter<PingReport>> _exporters;

        public PingCommand(IPingService pingService, IEnumerable<IReportExporter<PingReport>> exporters)
            => (_pingService, _exporters) = (pingService, exporters);

        protected override async Task<int> ExecuteAsync(
            CommandContext context, PingSettings settings, CancellationToken ct)
        {
            var samples = new List<PingMetrics>();
            var infinite = settings.Count == 0;

            var startedAt = DateTime.Now;
            var stopwatch = Stopwatch.StartNew();

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            ConsoleCancelEventHandler handler = (_, e) => { e.Cancel = true; cts.Cancel(); };
            Console.CancelKeyPress += handler;

            try
            {
                LiveTables.ShowPingHeader(settings.Host);

                while (!cts.Token.IsCancellationRequested &&
                       (infinite || samples.Count < settings.Count))
                {
                    var m = await _pingService.GetPingMetricsAsync(
                        settings.Host, settings.TimeoutMs, cts.Token);

                    samples.Add(m);
                    LiveTables.ShowPingSample(m);

                    if (infinite || samples.Count < settings.Count)
                        await Task.Delay(settings.IntervalMs, cts.Token);
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                Console.CancelKeyPress -= handler;
            }

            stopwatch.Stop();

            var statistics = PingStatistics.From(samples);
            LiveTables.ShowPingSummary(settings.Host, statistics);

            if (!string.IsNullOrWhiteSpace(settings.OutputPath))
                return await ExportAsync(settings, statistics, samples, startedAt, stopwatch.Elapsed, ct);

            return 0;
        }

        private async Task<int> ExportAsync(
            PingSettings settings, PingStatistics statistics, IReadOnlyList<PingMetrics> samples,
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

            var report = new PingReport(
                settings.Host, settings.TimeoutMs, settings.IntervalMs,
                startedAt, duration, statistics, samples);

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
