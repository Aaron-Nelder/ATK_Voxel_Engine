using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class BenchmarkManager
{
    static Dictionary<string, Benchmark> _benchmarks = new Dictionary<string, Benchmark>();
    public static void Remove(string name)
    {
        if (_benchmarks.ContainsKey(name))
            _benchmarks.Remove(name);
    }

    public static void Bench(Action proc, string name, LogType lType = LogType.Log, bool screenLog = true)
    {
        if (_benchmarks.ContainsKey(name))
        {
            if (screenLog)
                ScreenLogger.Log(LogType.Warning, $"{name}: ALREADY IN DICTIONARY");
            UnityEngine.Debug.LogWarning($"{name}: ALREADY IN DICTIONARY");
            return;
        }

        Benchmark b = new Benchmark(proc, name, lType, screenLog);
        _benchmarks.Add(name, b);
        b.Start();
    }
}

struct Benchmark
{
    Stopwatch _sw;
    Action _proc;
    string _name;
    LogType _lType;
    bool _screenLog;

    public Benchmark(Action proc, string name, LogType lType = LogType.Log, bool screenLog = true)
    {
        _sw = new Stopwatch();
        _proc = proc;
        _name = name;
        _lType = lType;
        _screenLog = screenLog;
        _proc = proc;
    }

    public Benchmark Start(bool autoEnd = true)
    {
        _sw.Start();

        _proc?.Invoke();

        if (autoEnd)
            End();

        return this;
    }

    public Benchmark End()
    {
        _sw.Stop();

        if (_screenLog)
            ScreenLogger.Log(_lType,$"{_name}: {_sw.ElapsedMilliseconds}ms");

        UnityEngine.Debug.Log($"{_name}: {_sw.ElapsedMilliseconds}ms");

        BenchmarkManager.Remove(_name);
        return this;
    }
}
