using NetPulse_CLI.Core.Interfaces;
using NetPulse_CLI.Core.Models;
using NetPulse_CLI.Settings;
using Spectre.Console;
using Spectre.Console.Cli;

namespace NetPulse_CLI.Commands
{
    public sealed class ScanCommand : AsyncCommand<ScanSettings>
    {
        private readonly IPortScanner _scanner;

        public ScanCommand(IPortScanner scanner) => _scanner = scanner;

        protected override async Task<int> ExecuteAsync(CommandContext context, 
            ScanSettings settings, CancellationToken cancellationToken)
        {
            var total = settings.ToPort - settings.FromPort + 1;
            IReadOnlyList<ScanResult> results = [];

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

            return 0;
        }
    }   
}
