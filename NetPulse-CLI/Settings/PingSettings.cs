using Spectre.Console.Cli;
using Spectre.Console;
using System.ComponentModel;

namespace NetPulse_CLI.Settings
{
    public sealed class PingSettings : CommandSettings
    {
        [Description("Host or IP Adress to scan")]
        [CommandArgument(0, "<host>")]
        public string Host { get; init; } = string.Empty;

        [Description("Ping timeout ms")]
        [CommandOption("-t|--timeout")]
        [DefaultValue(2000)]
        public int TimeoutMs { get; init; }

        [Description("Pings Interval")]
        [CommandOption("-i|--interval")]
        [DefaultValue(1000)]
        public int IntervalMs { get; init; }

        [Description("Pings to send (0 = sending pings until Crtl+C)")]
        [CommandOption("-n|--count")]
        [DefaultValue(0)]

        public int Count { get; init; }

        public override ValidationResult Validate()
        {
            if (TimeoutMs <= 0)
            {
                return ValidationResult.Error("Timeout must be higher than 0");
            }
            if (IntervalMs < 0)
            {
                return ValidationResult.Error("Interval can't be negative");
            }
            return ValidationResult.Success();
        }
    }
}
