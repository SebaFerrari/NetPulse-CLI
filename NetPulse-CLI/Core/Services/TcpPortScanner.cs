using System.Collections.Concurrent;
using System.Net.Sockets;
using NetPulse_CLI.Core.Interfaces;
using NetPulse_CLI.Core.Models;

namespace NetPulse_CLI.Core.Services
{

    public class TcpPortScanner : IPortScanner
    {
        public async Task<ScanResult> ScanPortAsync(string host, int port, int timeoutMs, CancellationToken ct = default)
        {
            using TcpClient client = new();
            using CancellationTokenSource timeoutCts = new(timeoutMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource (ct, timeoutCts.Token);
            try
            {
                await client.ConnectAsync(host, port, linkedCts.Token);
                return new ScanResult(host, port, PortStatus.Open, DateTime.Now);
            }
            catch (SocketException)
            {
                return new ScanResult(host, port, PortStatus.Closed, DateTime.Now);
            }
            catch (OperationCanceledException)
            {
                if (ct.IsCancellationRequested) throw;
                return new ScanResult(host, port, PortStatus.Filtered, DateTime.Now);
            }
        }

        public async Task<IReadOnlyList<ScanResult>> ScanRangeAsync 
            (string host, int fromPort, int toPort, int timeoutMs, int concurrency,
            IProgress<ScanResult>? progress = null, CancellationToken ct = default)
        {
            var ports = Enumerable.Range(fromPort, toPort - fromPort + 1);
            var results = new ConcurrentBag<ScanResult>();

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = concurrency,
                CancellationToken = ct
            };

            await Parallel.ForEachAsync(ports, options, async (port, token) =>
            {
                var result = await ScanPortAsync(host, port, timeoutMs, token);
                results.Add(result);
                progress?.Report(result);
            });

            return results.OrderBy(r => r.Port).ToList();
        }
    }
}
