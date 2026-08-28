# NetPulse CLI ⚡

A high-performance, asynchronous networking & socket monitoring CLI built with **C# (.NET 8/9)** and **Spectre.Console**. 

NetPulse is designed for network administrators, developers, and DevOps engineers who need rapid TCP port discovery, continuous ICMP latency telemetry, and clean diagnostic reporting directly from their terminal or automated CI/CD pipelines.

---

## 🌟 Key Features

- **⚡ Asynchronous TCP Port Scanning:** High-speed socket probing with fine-grained per-port timeout controls and cancellation support.
- **🛡️ Controlled Concurrency:** Uses `SemaphoreSlim` and `Parallel.ForEachAsync` to balance maximum throughput while preventing OS socket exhaustion and file descriptor limits on Windows and Linux.
- **📡 Live ICMP Latency Probing:** Continuous ping polling with real-time tracking of Round-Trip Time (RTT), packet jitter, and packet loss metrics.
- **🎨 Rich Interactive Terminal UI:** Built with **Spectre.Console**, featuring live-updating ANSI data tables, multi-task progress bars, and colored diagnostic status indicators.
- **📊 Diagnostic Report Exporting:** Cleanly export telemetry and scan audits directly to **JSON** and **CSV** formats for post-mortem analysis.
- **🐳 Single-File Binary & Docker Ready:** Distributable as a zero-dependency self-contained executable or a lightweight Docker container.
