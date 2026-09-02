using Spectre.Console;


namespace NetPulse_CLI.UI
{
    public static class InteractiveMenu
    {
        public static string[]? BuildArgs()
        {
            AnsiConsole.MarkupLine("[grey]Network Monitoring CLI - interactive mode[/]");

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>().Title("What do you want to do?").AddChoices("Scan TCP ports",
                "Probe ICMP latency", "Exit")
                );

            return action switch
            {
                "Scan TCP ports" => BuildScanArgs(),
                "Probe ICMP latency" => BuildPingArgs(),
                _ => null
            };
        }

        private static string[]? BuildScanArgs()
        {
            var host = AskHost();
            var preset = AnsiConsole.Prompt(
                new SelectionPrompt<string>().Title("Which ports?").AddChoices(
                    "Well-known ports [grey](1 - 1024)[/] - recommended",
                    "Full scan [grey](1 - 65535)[/] - can take several minutes",
                    "Custom range",
                    "A single port"));

            int from, to;

            switch (preset)
            {
                case "Well-known ports [grey](1 - 1024)[/] - recommended":
                    from = 1; to = 1024;
                    break;

                case "Full scan [grey](1 - 65535)[/] - can take several minutes":
                    if (!AnsiConsole.Confirm("This may take several minutes. Continue?", defaultValue: false))
                        return null;
                    from = 1; to = 65535;
                    break;

                case "A single port":
                    from = to = AskPort("Port to check");
                    break;

                default:
                    from = AskPort("From port");
                    to = AskPort("To port", minimum: from);
                    break;
            }

            var timeout = AnsiConsole.Prompt(
                new TextPrompt<int>("Timeout per port (ms): ").DefaultValue(1000)
                .Validate(t => t > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Must be greater than 0[/]")));

            var args = new List<string>
            {
                "scan", host,
                "--from", from.ToString(),
                "--to", to.ToString(),
                "--timeout", timeout.ToString()
            };

            AddOutputIfRequested(args);
            return args.ToArray();
        }

        private static string[] BuildPingArgs()
        {
            var host = AskHost(defaultHost: "8.8.8.8");

            var count = AnsiConsole.Prompt(
                new TextPrompt<int>("How many pings? [grey](0 = continuous, Ctrl+C to stop)[/]:")
                .DefaultValue(0).Validate(p => p >= 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Can't be negative[/]")));

            var interval = AnsiConsole.Prompt(
                new TextPrompt<int>("Interval between pings (ms): ")
                .DefaultValue(1000).Validate(i => i>0 ? ValidationResult.Success() : ValidationResult.Error("[red]Must be greater than 0[/]")));

            var args = new List<string>
            {
                "ping", host,
                "--count", count.ToString(),
                "--interval", interval.ToString()
            };

            AddOutputIfRequested(args);
            return args.ToArray();
        }

        private static string AskHost(string defaultHost = "127.0.0.1") 
        { 
            return AnsiConsole.Prompt(
                new TextPrompt<string>("Host or IP address: ").DefaultValue(defaultHost).Validate(
                    h => !string.IsNullOrWhiteSpace(h) ? ValidationResult.Success() : ValidationResult.Error("[red]Host can't be empty[/]")));
        }

        private static int AskPort(string label, int minimum = 1)
        {
            return AnsiConsole.Prompt(
                new TextPrompt<int>($"{label} [grey]({minimum}-65535)[/]: ").Validate(
                    p => p >= minimum && p <= 65535 ? ValidationResult.Success() : ValidationResult.Error($"[red]Must be between {minimum} and 65535")));
        }

        private static void AddOutputIfRequested(List<string> args)
        {
            if (!AnsiConsole.Confirm("Save report to a file?", defaultValue: false)) return;

            var format = AnsiConsole.Prompt(
                new SelectionPrompt<string>().Title("Format?").AddChoices("json", "csv"));

            var path = AnsiConsole.Prompt(
                new TextPrompt<string>("File path: ").DefaultValue($"netpulse-report.{format}"));

            if (!path.EndsWith($".{format}", StringComparison.OrdinalIgnoreCase)) path += $".{format}";

            args.Add("--output");
            args.Add(path);
        }
    }
}
