using Spectre.Console;
using Spectre.Console.Cli;
using NetPulse_CLI.Settings;
using NetPulse_CLI.Core.Interfaces;
using NetPulse_CLI.Core.Models;

namespace NetPulse_CLI.Commands
{
    public sealed class PingCommand : AsyncCommand<PingSettings>
    {
        private readonly IPingService _pingService;
        public PingCommand (IPingService pingService) => _pingService = pingService;

        protected override async Task<int> ExecuteAsync(CommandContext context, 
            PingSettings settings, CancellationToken ct)
        {
            var list = new List<PingMetrics>();
            var infinite = settings.Count == 0;
            using var cts = CancellationTokenSource.CreateLinkedTokenSource (ct);
            ConsoleCancelEventHandler handler = (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            Console.CancelKeyPress += handler;

            AnsiConsole.MarkupLine($"Probing [bold]{Markup.Escape(settings.Host)}[/]  ...(Ctrl+C to stop)");
            try
            {
                while (!cts.Token.IsCancellationRequested && (infinite || list.Count < settings.Count))
                {
                    var l = await _pingService.GetPingMetricsAsync(settings.Host, settings.TimeoutMs, cts.Token);
                    list.Add(l);
                    if (l.Success) AnsiConsole.MarkupLine($"{l.RoundtripTimeMs}ms");
                    else AnsiConsole.MarkupLine($"[red] No response [/] ([grey]{l.Status}[/])");
                    if (infinite || list.Count < settings.Count)
                    {
                        await Task.Delay(settings.IntervalMs, cts.Token);
                    }
                }
            }
            catch (OperationCanceledException) { }
            finally { Console.CancelKeyPress -= handler; }
            Summary(settings.Host, list);
            return 0;
        }

        private void handler(object? sender, ConsoleCancelEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void Summary(string host, List<PingMetrics> list)
        {
            if (list.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow] No samples record [/]");
                return;
            }

            var succeded = list.Where(l => l.Success).ToList();
            var lost = list.Count - succeded.Count;
            var succededPer = 100.0 * (succeded.Count / list.Count);
            var lostPer = 100.0 * ((list.Count - succeded.Count) / list.Count);

            AnsiConsole.MarkupLine($"[bold]{Markup.Escape(host)} Statistics:[/]");
            AnsiConsole.MarkupLine($"[bold] Sent: {list.Count}[/]");
            AnsiConsole.MarkupLine($"[green] Received: {succeded.Count} | {succededPer}%[/]");
            AnsiConsole.MarkupLine($"[red] Lost: {lost} | {lostPer}%[/]");

            if (succeded.Count > 0)
            {
                var rtts = succeded.Select(l => l.RoundtripTimeMs!.Value).ToList();
                AnsiConsole.MarkupLine($"RTTS - min: {rtts.Min()}ms, max: {rtts.Max()}ms, avg: {rtts.Average():F1}ms");
            }
        }
    }
}
