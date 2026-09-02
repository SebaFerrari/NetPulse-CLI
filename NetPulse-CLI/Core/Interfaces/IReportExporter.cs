using NetPulse_CLI.Core.Models;
using NetPulse_CLI.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetPulse_CLI.Core.Interfaces
{
    public interface IReportExporter<T>
    {
        string Format { get; }
        Task ExportAsync(T report, string path, CancellationToken ct = default);

    }
}
