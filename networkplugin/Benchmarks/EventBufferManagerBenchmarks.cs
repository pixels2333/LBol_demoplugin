using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NetworkPlugin.Core;

namespace NetworkPlugin.Benchmarks;

public class EventBufferManagerBenchmarks
{
    private NetworkEventBufferManager _mgr = null!;
    private List<Dictionary<string, object>> _events = null!;

    [Params(10, 100, 1000)]
    public int EventCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _mgr = new NetworkEventBufferManager();
        _events = new List<Dictionary<string, object>>(EventCount);
        long now = DateTime.Now.Ticks;

        for (int i = 0; i < EventCount; i++)
        {
            _events.Add(new Dictionary<string, object>
            {
                ["EventType"] = $"Event_{i}",
                ["Payload"] = new string('x', 100),
                ["Timestamp"] = now + i,
                ["PlayerName"] = "Player_" + (i % 10)
            });
        }
    }

    [Benchmark]
    public void EnqueueEvents()
    {
        foreach (var evt in _events)
            _mgr.EnqueueEvent(evt);
    }

    [Benchmark]
    public void EnqueueAndProcess()
    {
        foreach (var evt in _events)
            _mgr.EnqueueEvent(evt);

        _mgr.ProcessBufferedEvents(_ => { });
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<EventBufferManagerBenchmarks>();
        Console.WriteLine(summary);
    }
}
