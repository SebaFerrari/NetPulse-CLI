using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using NetPulse_CLI.Core.Interfaces;
using NetPulse_CLI.Core.Models;
using NetPulse_CLI.Settings;

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
    }
}
