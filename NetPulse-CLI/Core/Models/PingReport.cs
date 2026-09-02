namespace NetPulse_CLI.Core.Models
{
    public record PingReport (string Host, int TimeoutMs, int IntervalMs,
    DateTime StartedAt, TimeSpan Duration,
    PingStatistics Statistics,
    IReadOnlyList<PingMetrics> Samples)
    {
    }

    public record PingStatistics (int Sent, int Received, int Lost, double LossPercentage,
    long? MinRttMs, double? AvgRttMs, long? MaxRttMs, double? JitterMs)
    {
        public static PingStatistics From(IReadOnlyList<PingMetrics> samples)
        {
            var ok = samples.Where(s => s.Success).ToList();
            var rtts = ok.Select(s => s.RoundtripTimeMs!.Value).ToList();

            double? jitter = rtts.Count > 1
                ? rtts.Zip(rtts.Skip(1), (a, b) => Math.Abs(b - a)).Average()
                : null;

            return new PingStatistics(
                samples.Count,
                ok.Count,
                samples.Count - ok.Count,
                samples.Count == 0 ? 0 : 100.0 * (samples.Count - ok.Count) / samples.Count,
                rtts.Count > 0 ? rtts.Min() : null,
                rtts.Count > 0 ? rtts.Average() : null,
                rtts.Count > 0 ? rtts.Max() : null,
                jitter);
        }
    };
}