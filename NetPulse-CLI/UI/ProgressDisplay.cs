
using NetPulse_CLI.Core.Models;
using Spectre.Console;

namespace NetPulse_CLI.UI
{
    public class ProgressDisplay
    {
        public static async Task<T> RunWithProgressAsync<T>(
            string description,
            int total,
            Func<IProgress<ScanResult>, Task<T>> work)
        {
            T result = default!;

            await AnsiConsole.Progress().StartAsync(async ctx =>
            {
                var task = ctx.AddTask(description, maxValue: total);
                var progress = new Progress<ScanResult>(_ => task.Increment(1));
                result = await work(progress);
            });

            return result;
        }
        public static async Task<T> RunWithStatusAsync<T>(string message, Func<Task<T>> work)
    => await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .StartAsync(message, async _ => await work());
    }
}
