using NetPulse_CLI.Core.Models;
using Spectre.Console;

namespace NetPulse_CLI.UI
{
    public static class LiveTables
    {
        private static readonly Dictionary<int, string> ServiceNames = new()
        {
            [21] = "FTP",
            [22] = "SSH",
            [23] = "Telnet",
            [25] = "SMTP",
            [53] = "DNS",
            [80] = "HTTP",
            [110] = "POP3",
            [135] = "RPC",
            [139] = "NetBIOS",
            [143] = "IMAP",
            [443] = "HTTPS",
            [445] = "SMB",
            [1433] = "SQL Server",
            [3306] = "MySQL",
            [3389] = "RDP",
            [5432] = "PostgreSQL",
            [6379] = "Redis",
            [8080] = "HTTP alt"
        };

        public static void ShowOpenPorts(IReadOnlyList<ScanResult> open)
        {
            if (open.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No open ports found.[/]");
                return;
            }

            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Port")
                .AddColumn("Service")
                .AddColumn("Status");

            foreach (var r in open.OrderBy(r => r.Port))
                table.AddRow(
                    r.Port.ToString(),
                    ServiceNames.TryGetValue(r.Port, out var name) ? name : "[grey]unknown[/]",
                    Describe(r.Status));

            AnsiConsole.Write(table);
        }

        public static void ShowScanSummary(string host, IReadOnlyList<ScanResult> results, TimeSpan duration)
        {
            var open = results.Count(r => r.Status == PortStatus.Open);
            var closed = results.Count(r => r.Status == PortStatus.Closed);
            var filtered = results.Count(r => r.Status == PortStatus.Filtered);

            var panel = new Panel(
                $"Host: [bold]{Markup.Escape(host)}[/]\n" +
                $"Scanned: {results.Count} ports in {duration.TotalSeconds:F1}s\n" +
                $"[green]{open} open[/] · [grey]{closed} closed[/] · [yellow]{filtered} no response[/]")
                .Header("[bold]Scan summary[/]")
                .Border(BoxBorder.Rounded);

            AnsiConsole.Write(panel);
        }

        public static string Describe(PortStatus status) => status switch
        {
            PortStatus.Open => "[green]Open[/]",
            PortStatus.Closed => "[grey]Closed[/]",
            PortStatus.Filtered => "[yellow]No response (possible firewall)[/]",
            _ => "[grey]Unknown[/]"
        };

        public static void ShowPingSample(PingMetrics m)
        {
            if (m.Success)
                AnsiConsole.MarkupLine($"  reply in [green]{m.RoundtripTimeMs} ms[/]");
            else
                AnsiConsole.MarkupLine($"  [red]no response[/] ([grey]{m.Status}[/])");
        }

        public static void ShowPingSummary(string host, PingStatistics stats)
        {
            if (stats.Sent == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No samples recorded.[/]");
                return;
            }

            var body = $"Host: [bold]{Markup.Escape(host)}[/]\n" +
                       $"Sent: {stats.Sent} · Received: {stats.Received} · Loss: {stats.LossPercentage:F1}%";

            if (stats.Received > 0)
            {
                body += $"\nRTT — min: {stats.MinRttMs} ms · avg: {stats.AvgRttMs:F1} ms · max: {stats.MaxRttMs} ms";

                if (stats.JitterMs is not null)
                    body += $"\nJitter: {stats.JitterMs:F1} ms";
            }

            AnsiConsole.Write(new Panel(body)
                .Header("\n[bold]Ping statistics[/]")
                .Border(BoxBorder.Rounded));
        }

        public static void ShowPingHeader(string host)
        {
            AnsiConsole.Write(new Rule($"[bold]Probing {Markup.Escape(host)}[/]").LeftJustified());
            AnsiConsole.MarkupLine("[grey]Press Ctrl+C to stop[/]");
        }

        public static void ShowCancelled(string what)
            => AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(what)} cancelled by user.[/]");

        public static void ShowReportSaved(string fullPath)
            => AnsiConsole.MarkupLine($"Report saved to [blue]{Markup.Escape(fullPath)}[/]");

        public static void ShowError(string message)
            => AnsiConsole.MarkupLine($"[red]{Markup.Escape(message)}[/]");
    }
}
