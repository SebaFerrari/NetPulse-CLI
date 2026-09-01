using NetPulse_CLI.Core.Interfaces;
using NetPulse_CLI.Core.Models;
using NetPulse_CLI.Settings;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics;

namespace NetPulse_CLI.Commands
{
    public sealed class ScanCommand : AsyncCommand<ScanSettings>
    {
        private readonly IPortScanner _scanner;
        private readonly IEnumerable<IReportExporter> _exporters;
        public ScanCommand(IPortScanner scanner, IEnumerable<IReportExporter> exporters)
            => (_scanner, _exporters) = (scanner, exporters);

        protected override async Task<int> ExecuteAsync(CommandContext context, 
            ScanSettings settings, CancellationToken cancellationToken)
        {
            var total = settings.ToPort - settings.FromPort + 1;
            IReadOnlyList<ScanResult> results = [];
            var startedAt = DateTime.Now;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await AnsiConsole.Progress()
                    .StartAsync(async ctx =>
                    {
                        var task= ctx.AddTask("[green]Scanning ports[/]", maxValue: total);

                        var progress = new Progress<ScanResult>(_ => task.Increment(1));

                        results = await _scanner.ScanRangeAsync(
                            settings.Host,
                            settings.FromPort,
                            settings.ToPort,
                            settings.TimeoutMs,
                            settings.Concurrency,
                            progress,
                            cancellationToken);
                    });
            }
            catch (OperationCanceledException)
            {
                AnsiConsole.MarkupLine("[yellow]Scan cancelled by user.[/]");
                return 1;
            }

            var open = results.Where(r => r.Status == PortStatus.Open).ToList();

            AnsiConsole.MarkupLine(
                $"scanned [bold]{results.Count}[/] ports in {settings.Host}: " +
                $"[green]{open.Count} open[/].");

            foreach (var r in open)
                AnsiConsole.MarkupLine($"Port [green]{r.Port}[/] open");

            stopwatch.Stop();

            if (!string.IsNullOrWhiteSpace(settings.OutputPath))
            {
                var report = new ScanReport(
                    settings.Host,
                    settings.FromPort,
                    settings.ToPort,
                    startedAt,
                    stopwatch.Elapsed,
                    results);

                var extension = Path.GetExtension(settings.OutputPath).TrimStart('.').ToLowerInvariant();
                var exporter = _exporters.FirstOrDefault(e => e.Format == extension);

                if (exporter is null)
                {
                    AnsiConsole.MarkupLine(
                        $"[red]Unsupported format '{Markup.Escape(extension)}'.[/] " +
                        $"Available: {string.Join(", ", _exporters.Select(e => e.Format))}");
                    return 1;
                }

                var directory = Path.GetDirectoryName(Path.GetFullPath(settings.OutputPath));
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                await exporter.ExportAsync(report, settings.OutputPath, cancellationToken);

                AnsiConsole.MarkupLine(
                    $"Report saved to [blue]{Markup.Escape(Path.GetFullPath(settings.OutputPath))}[/]");
            }

            return 0;
        }
    }   
}
