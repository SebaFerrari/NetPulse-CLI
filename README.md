# NetPulse CLI ⚡

An asynchronous network monitoring CLI built with **C# (.NET 10)** and **Spectre.Console**.

NetPulse answers two diagnostic questions from your terminal: **which TCP ports a host has open**, and **how well a host responds over time**. Both can be exported as JSON or CSV reports.

It ships in two flavours of the same engine — a flag-driven mode for scripts and CI pipelines, and a guided interactive menu for everyone else.

---

## 🌟 Features

- **⚡ Asynchronous TCP port scanning** — non-blocking socket probing with a configurable per-port timeout and full cancellation support.
- **🛡️ Controlled concurrency** — `Parallel.ForEachAsync` with a configurable `MaxDegreeOfParallelism`, preventing ephemeral-port and socket-descriptor exhaustion on Windows and Linux.
- **📡 ICMP latency probing** — one-shot or continuous polling with RTT, packet loss and jitter statistics.
- **🎨 Rich terminal UI** — built with Spectre.Console: progress bars, tables, summary panels and colour-coded status.
- **🧭 Interactive mode** — run it with no arguments and it walks you through the options with validated prompts and port presets.
- **📊 Report exporting** — JSON (full report with metadata and statistics) or CSV (flat table, ready for a spreadsheet).
- **📦 Single-file binary** — publishes as a self-contained executable that runs on machines without .NET installed.

---

## 📥 Installation

Download the latest `netpulse-vX.Y.Z-win-x64.exe` from the [Releases](../../releases) page. It is a single self-contained file — no installer, no .NET runtime required.

> **Note on the Windows SmartScreen warning:** the binary is not code-signed, so Windows may show an "unknown publisher" prompt the first time you run it. Click *More info → Run anyway*. You can verify the download against the SHA-256 checksum published in the release notes.

Verify the checksum in PowerShell:

```powershell
Get-FileHash .\netpulse.exe -Algorithm SHA256
```

---

## 🚀 Usage

### Interactive mode

Run the executable with no arguments and NetPulse guides you through it:

```bash
netpulse
```

You pick the action, the target, and a port preset (well-known ports, full scan, custom range or a single port). Every prompt is validated, so you cannot enter an invalid value.

### Command mode

```bash
# Scan the well-known ports of a host
netpulse scan 192.168.1.1

# Scan a custom range with a shorter timeout and save a report
netpulse scan 192.168.1.1 --from 20 --to 100 -t 500 -o report.json

# Send 10 pings half a second apart and export the samples as CSV
netpulse ping 8.8.8.8 -n 10 -i 500 -o latency.csv

# Continuous latency monitoring (Ctrl+C stops it and prints the statistics)
netpulse ping 8.8.8.8
```

Run `netpulse --help`, `netpulse scan --help` or `netpulse ping --help` for the built-in reference.

---

## ⚙️ Options

### `scan`

| Option | Default | Description |
|---|---|---|
| `<host>` | — | Host or IP address to scan (required) |
| `-f`, `--from` | `1` | First port in the range |
| `-T`, `--to` | `1024` | Last port in the range |
| `-t`, `--timeout` | `1000` | Milliseconds to wait per port |
| `-c`, `--concurrency` | `100` | Maximum simultaneous connections |
| `-o`, `--output` | — | Report path — format inferred from the extension |

### `ping`

| Option | Default | Description |
|---|---|---|
| `<host>` | — | Host or IP address to probe (required) |
| `-n`, `--count` | `0` | Number of pings — `0` means continuous until Ctrl+C |
| `-i`, `--interval` | `1000` | Milliseconds between pings |
| `-t`, `--timeout` | `2000` | Milliseconds to wait for each reply |
| `-o`, `--output` | — | Report path — format inferred from the extension |

---

## 📖 Reading the results

### Port states

| State | Meaning |
|---|---|
| **Open** | The TCP handshake completed — something is listening on that port. |
| **Closed** | The host actively refused the connection with an RST. The machine is up; nothing is listening there. |
| **No response** | Nothing came back before the timeout — usually a firewall dropping packets silently, or an unreachable host. |

The distinction between *closed* and *no response* is the diagnostic value of the scan: the first tells you the host is reachable, the second does not.

> ⚠️ **A timeout that is too short makes everything look filtered.** If you lower `--timeout` below what your network needs, closed ports will be reported as "no response". The default of 1000 ms is comfortable on a LAN; raise it for hosts across the internet.

### Ping statistics

Every run reports packets sent and received, loss percentage, and min / average / max RTT. **Jitter** is the average absolute difference between consecutive latencies — a low RTT with high jitter means an unstable connection, which matters for voice, video and gaming far more than raw latency does.

---

## 📄 Report formats

### JSON — the complete report

Includes the parameters used, timing metadata and, for ping, the computed statistics. Enum values are serialised as readable text rather than numbers.

```json
{
  "host": "8.8.8.8",
  "timeoutMs": 2000,
  "intervalMs": 1000,
  "startedAt": "2026-09-02T21:15:00.0000000-03:00",
  "duration": "00:00:05.0231044",
  "statistics": {
    "sent": 5,
    "received": 4,
    "lost": 1,
    "lossPercentage": 20.0,
    "minRttMs": 38,
    "avgRttMs": 39.5,
    "maxRttMs": 41,
    "jitterMs": 2.33
  },
  "samples": [
    { "host": "8.8.8.8", "success": true, "roundtripTimeMs": 39, "status": "Success", "timestamp": "..." }
  ]
}
```

### CSV — a flat table for spreadsheets

One row per result. Timestamps are ISO 8601 with invariant culture, so the file reads identically on any machine regardless of regional settings.

```csv
host,timestamp,success,rttMs,status
8.8.8.8,2026-09-02T21:15:00.0000000-03:00,true,39,Success
8.8.8.8,2026-09-02T21:15:01.0000000-03:00,false,,TimedOut
```

CSV cannot carry the metadata or the statistics block — it is a flat table by design. Use JSON when you want the full report, CSV when you want to chart the raw samples.

---

## 🔢 Exit codes

| Code | Meaning |
|---|---|
| `0` | Completed successfully. A continuous ping stopped with Ctrl+C also exits `0` — that is its normal way to end. |
| `1` | The scan was cancelled before finishing, or the requested export format is not supported. |

Invalid arguments are rejected by the parser before anything runs.

---

## 🧠 How it works

**Port scanning** uses a TCP *connect scan*: NetPulse attempts a real TCP handshake against each port and classifies the outcome. It sends no payload and reads no data — it does not need to know what protocol the service behind the port speaks.

**Latency probing** uses ICMP Echo Request/Reply via `System.Net.NetworkInformation.Ping`. Unlike the TCP case, a timeout here is not an exception: it comes back as a reply with a `TimedOut` status. Exceptions are reserved for failures that prevent the attempt entirely, such as DNS resolution errors.

**Concurrency** is capped because each connection consumes a socket descriptor and an ephemeral source port, and closed ports linger in `TIME_WAIT` for a couple of minutes before being reusable. Scanning a large range without a limit exhausts them faster than they are released, and connections start failing for reasons unrelated to the port's actual state — producing results that look valid and are not.

---

## 🏗️ Architecture

```
NetPulse-CLI/
├── Core/
│   ├── Models/          Immutable records: ScanResult, ScanReport, PingMetrics, PingReport
│   ├── Interfaces/      Contracts: IPortScanner, IPingService, IReportExporter<T>
│   └── Services/        Implementations: TcpPortScanner, IcmpPingService, exporters
├── Settings/            Command settings with validation
├── Commands/            Orchestration: ScanCommand, PingCommand
├── UI/                  Presentation: tables, progress, interactive menu
├── Infrastructure/      DI bridge for Spectre.Console.Cli
└── Program.cs           Composition root
```

The project is layered so that dependencies point inward: models know nothing, interfaces reference models, services implement interfaces, and the CLI layer depends on interfaces only. No service knows a console exists, and no command opens a socket.

---

## 🔨 Building from source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/<your-user>/NetPulse-CLI.git
cd NetPulse-CLI

# Run in development
dotnet run --project NetPulse-CLI -- scan 127.0.0.1 --to 200

# Publish a self-contained single-file executable
dotnet publish NetPulse-CLI/NetPulse-CLI.csproj -c Release -r win-x64
```

The binary lands in `NetPulse-CLI/bin/Release/net10.0/win-x64/publish/`.

> Do not enable `PublishTrimmed`. Both Spectre.Console.Cli and the dependency-injection container resolve types by reflection, which the trimmer cannot see — the build succeeds and the executable fails at runtime.

---

## 🗺️ Roadmap

- [ ] Docker image, with documented caveats around network namespaces and `CAP_NET_RAW` for ICMP
- [ ] Linux and macOS builds
- [ ] `--status` filter to export only selected port states
- [ ] Live-updating ping view instead of an appending log
- [ ] Service-name detection via banner grabbing on open ports

---

## ⚖️ Legal notice

**Scan only hosts you own or have explicit written authorisation to test.**

Port scanning systems without permission is illegal in many jurisdictions and violates the terms of service of virtually every hosting provider and network operator. Aggressive scanning is indistinguishable from an attack to intrusion-detection systems and may get your address blocked or reported.

This tool is provided for diagnostics on your own infrastructure and for educational purposes. You are responsible for how you use it.

---

## 📜 License

MIT — see [LICENSE](LICENSE).
