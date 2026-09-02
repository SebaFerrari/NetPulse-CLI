namespace NetPulse_CLI.Core.Interfaces
{
    public interface IReportExporter<T>
    {
        string Format { get; }
        Task ExportAsync(T report, string path, CancellationToken ct = default);
    }
}
