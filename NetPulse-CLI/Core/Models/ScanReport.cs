namespace NetPulse_CLI.Core.Models
{
    public record ScanReport(string Host, int FromPort, int ToPort, DateTime StartedAt, TimeSpan Duration,
        IReadOnlyList<ScanResult> Results);
}
