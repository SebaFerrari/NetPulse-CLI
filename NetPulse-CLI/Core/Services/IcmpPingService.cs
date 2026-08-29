using System.Runtime.CompilerServices;
using NetPulse_CLI.Core.Interfaces;
using NetPulse_CLI.Core.Models;
using System.Net.NetworkInformation;

namespace NetPulse_CLI.Core.Services
{
    public class IcmpPingService : IPingService
    {
        public async Task<PingMetrics> GetPingMetricsAsync(string host, int timeoutMs, CancellationToken ct = default)
        {
            using Ping pingSender = new();
            try
            {
                PingReply reply = await pingSender.SendPingAsync(host,TimeSpan.FromMilliseconds(timeoutMs), cancellationToken: ct);

                bool success = reply.Status == IPStatus.Success;

                return new PingMetrics(host, success, success ? reply.RoundtripTime : null, reply.Status, DateTime.Now);   
            }

            catch (PingException)
            {
                return new PingMetrics(host, false, null, IPStatus.Unknown, DateTime.Now);
            }
        }
    }
}
