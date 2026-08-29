using NetPulse_CLI.Core.Services;

namespace NetPulse_CLI
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var scanner = new TcpPortScanner();
            var ping = new IcmpPingService();

            Console.WriteLine(await scanner.ScanPortAsync("192.168.116.1", 139, 1000));
            Console.WriteLine(await scanner.ScanPortAsync("1.1.1.1", 81, 5000));
            Console.WriteLine(await ping.GetPingMetricsAsync("8.8.8.8", 1000));
        }
    }
}
