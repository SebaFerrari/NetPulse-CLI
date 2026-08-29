using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace NetPulse_CLI.Settings
{
    public sealed class ScanSettings : CommandSettings
    {
        [Description("Host or IP Address to scan")]
        [CommandArgument(0, "<host>")]
        public string Host { get; init; } = string.Empty;

        [Description("Initial Port")]
        [CommandOption("-f|--from")]
        [DefaultValue(1)]
        public int FromPort { get; init; }

        [Description("Final Port")]
        [CommandOption("-T|--to")]
        [DefaultValue(1024)]
        public int ToPort { get; init; }

        [Description("Timeout ms")]
        [CommandOption("-t|--timeout")]
        [DefaultValue(1000)]
        public int TimeoutMs { get; init; }

        [Description("Maximum Concurrency")]
        [CommandOption("-c|--concurrency")]
        [DefaultValue(100)]
        public int Concurrency { get; init; }

        public override ValidationResult Validate()
        {
            if (FromPort < 0 || ToPort > 65535) 
            {
                return ValidationResult.Error("Ports have to be between 1 & 65535");
            }
            if (FromPort > ToPort)
            {
                return ValidationResult.Error("Initial Port can't be higher than final Port");
            }
            if (TimeoutMs <= 0)
            {
                return ValidationResult.Error("Timeout has to be higher than 0");
            }
            if (Concurrency < 1)
            {
                return ValidationResult.Error("Concurrency has to be at least 1");
            }
            return ValidationResult.Success();
        }
    }
}
