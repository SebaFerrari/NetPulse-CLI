namespace NetPulse_CLI.Core.Models
{
    public enum PortStatus { Open, Closed, Filtered }

    public record ScanResult(string Host, int Port, PortStatus Status, DateTime Timestamp);
}
