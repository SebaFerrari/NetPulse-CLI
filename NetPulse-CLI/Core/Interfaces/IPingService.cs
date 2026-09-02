using NetPulse_CLI.Core.Models;

namespace NetPulse_CLI.Core.Interfaces
{
    public interface IPingService
    {
        Task<PingMetrics> GetPingMetricsAsync(string host, int timeoutMs, CancellationToken ct = default);
    }
}
