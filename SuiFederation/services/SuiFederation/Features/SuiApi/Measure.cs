using System;
using System.Diagnostics;
using Beamable.Common;

namespace Beamable.SuiFederation.Features.SuiApi;

public class Measure : IDisposable
{
    private readonly string _operationName;
    private readonly Stopwatch _watch;

    public Measure(string operationName)
    {
        this._operationName = operationName;
        BeamableLogger.Log("Starting {operation}", operationName);
        _watch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        _watch.Stop();
        BeamableLogger.Log("Done executing {operation} in {elapsedSec} sec", _operationName, _watch.Elapsed.TotalSeconds.ToString("0.####"));
    }
}