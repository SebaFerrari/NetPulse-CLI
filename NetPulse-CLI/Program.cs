using Microsoft.Extensions.DependencyInjection;
using NetPulse_CLI.Commands;
using NetPulse_CLI.Core.Interfaces;
using NetPulse_CLI.Core.Models;
using NetPulse_CLI.Core.Services;
using NetPulse_CLI.Infrastructure;
using NetPulse_CLI.UI;
using Spectre.Console;
using Spectre.Console.Cli;

var services = new ServiceCollection();
services.AddSingleton<IPortScanner, TcpPortScanner>();
services.AddSingleton<IPingService, IcmpPingService>();
services.AddSingleton<IReportExporter<ScanReport>, JsonReportExporter>();
services.AddSingleton<IReportExporter<ScanReport>, CsvReportExporter>();
services.AddSingleton<IReportExporter<PingReport>, JsonReportExporter>();
services.AddSingleton<IReportExporter<PingReport>, CsvReportExporter>();

var app = new CommandApp(new TypeRegistrar(services));

app.Configure(config =>
{
    config.SetApplicationName("netpulse");

    config.AddCommand<ScanCommand>("scan")
        .WithDescription("Scans a range of TCP ports on a host")
        .WithExample("scan", "192.168.1.1")
        .WithExample("scan", "192.168.1.1", "--from", "20", "--to", "100", "-t", "500");

    config.AddCommand<PingCommand>("ping")
        .WithDescription("Continuously probes ICMP latency to a host")
        .WithExample("ping", "8.8.8.8")
        .WithExample("ping", "8.8.8.8", "-n", "10", "-i", "500");
});

var launchedInteractively = args.Length == 0;

if (launchedInteractively)
{
    var interactive = InteractiveMenu.BuildArgs();
    if (interactive is null) return 0;
    args = interactive;
}

var exitCode = await app.RunAsync(args);

if (launchedInteractively && !Console.IsInputRedirected)
{
    AnsiConsole.MarkupLine("\n[grey]Press any key to exit...[/]");
    Console.ReadKey(intercept: true);
}

return exitCode;
