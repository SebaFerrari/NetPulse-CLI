using NetPulse_CLI.Core.Models;

namespace NetPulse_CLI.Core.Interfaces
{
    public interface IPortScanner
    {
        Task<ScanResult> ScanPortAsync(string host, int port, int timeoutMs, CancellationToken ct = default);
    }
}
