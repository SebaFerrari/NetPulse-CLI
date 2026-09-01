using NetPulse_CLI.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetPulse_CLI.Core.Interfaces
{
    public interface IReportExporter
    {
        string Format { get; }                                    // "json", "csv"
        Task ExportAsync(ScanReport report, string path, CancellationToken ct = default);
    }
}
