//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Concurrent;
using System.Diagnostics;

namespace NosCore.PathFinder.Api;

public class PerformanceTracker
{
    private readonly ConcurrentQueue<PerformanceEntry> _brushfireEntries = new();
    private readonly ConcurrentQueue<PerformanceEntry> _flowfieldEntries = new();
    private const int MaxEntries = 1000;

    private readonly Process _process = Process.GetCurrentProcess();
    private TimeSpan _lastCpuTime = TimeSpan.Zero;
    private DateTime _lastCpuCheck = DateTime.UtcNow;
    private double _cpuUsage = 0;

    public void RecordBrushfire(TimeSpan elapsed, int cellCount)
    {
        _brushfireEntries.Enqueue(new PerformanceEntry(DateTime.UtcNow, elapsed, cellCount));
        TrimQueue(_brushfireEntries);
    }

    public void RecordFlowField(TimeSpan elapsed, int vectorCount)
    {
        _flowfieldEntries.Enqueue(new PerformanceEntry(DateTime.UtcNow, elapsed, vectorCount));
        TrimQueue(_flowfieldEntries);
    }

    public object GetStats()
    {
        var brushfireStats = CalculateStats(_brushfireEntries.ToArray());
        var flowfieldStats = CalculateStats(_flowfieldEntries.ToArray());
        UpdateCpuUsage();

        _process.Refresh();
        var memoryMb = _process.WorkingSet64 / 1024.0 / 1024.0;

        return new
        {
            Brushfire = brushfireStats,
            FlowField = flowfieldStats,
            TotalRequests = _brushfireEntries.Count + _flowfieldEntries.Count,
            CpuPercent = Math.Round(_cpuUsage, 1),
            MemoryMb = Math.Round(memoryMb, 1)
        };
    }

    private void UpdateCpuUsage()
    {
        var now = DateTime.UtcNow;
        var elapsed = now - _lastCpuCheck;
        if (elapsed.TotalMilliseconds < 500) return;

        _process.Refresh();
        var cpuTime = _process.TotalProcessorTime;
        var cpuDelta = cpuTime - _lastCpuTime;
        _cpuUsage = cpuDelta.TotalMilliseconds / elapsed.TotalMilliseconds / Environment.ProcessorCount * 100;

        _lastCpuTime = cpuTime;
        _lastCpuCheck = now;
    }

    private static object CalculateStats(PerformanceEntry[] entries)
    {
        if (entries.Length == 0)
        {
            return new { Count = 0, AvgMs = 0.0, MinMs = 0.0, MaxMs = 0.0, AvgCells = 0.0, CellsPerSecond = 0.0 };
        }

        var times = entries.Select(e => e.Elapsed.TotalMilliseconds).ToArray();
        var cells = entries.Select(e => e.CellCount).ToArray();
        var avgMs = times.Average();
        var avgCells = cells.Average();

        return new
        {
            Count = entries.Length,
            AvgMs = Math.Round(avgMs, 3),
            MinMs = Math.Round(times.Min(), 3),
            MaxMs = Math.Round(times.Max(), 3),
            P95Ms = Math.Round(Percentile(times, 95), 3),
            AvgCells = Math.Round(avgCells, 1),
            CellsPerSecond = avgMs > 0 ? Math.Round(avgCells / avgMs * 1000, 0) : 0
        };
    }

    private static double Percentile(double[] values, int percentile)
    {
        if (values.Length == 0) return 0;
        var sorted = values.OrderBy(v => v).ToArray();
        var index = (int)Math.Ceiling(percentile / 100.0 * sorted.Length) - 1;
        return sorted[Math.Max(0, index)];
    }

    private static void TrimQueue(ConcurrentQueue<PerformanceEntry> queue)
    {
        while (queue.Count > MaxEntries)
        {
            queue.TryDequeue(out _);
        }
    }

    private record PerformanceEntry(DateTime Timestamp, TimeSpan Elapsed, int CellCount);
}
