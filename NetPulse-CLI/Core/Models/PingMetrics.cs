using System.Net.NetworkInformation;

namespace NetPulse_CLI.Core.Models
{
    public record PingMetrics(string Host, bool Success, long? RoundTripTimeMs, IPStatus Status, DateTime TimeStamp);
}
