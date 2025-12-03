using System;
using System.Runtime.CompilerServices;
using System.Threading;
using MMOR.NET.RichString;

namespace MMOR.NET.MultiThreadMonteCarlo
{
  public abstract class SimulationObject<T> : IDisposable
    where T : SimulationObject<T>
  {
    //-+-+-+-+-+-+-+-+
    // Generic Data
    //-+-+-+-+-+-+-+-+
    private ulong _totalIterations;
    public ulong totalIterations => _totalIterations;
    private readonly ManualResetEventSlim _pauseGate = new(true);
    private readonly ManualResetEventSlim _processGate = new(true);
    private readonly SemaphoreSlim _processLock = new(1, 1);

    //-+-+-+-+-+-+-+-+
    // Pretty Print
    //-+-+-+-+-+-+-+-+
    public abstract IRichString PrettyPrintHeader();
    public abstract IRichString PrettyPrintBody();

    //-+-+-+-+-+-+-+-+
    // Methods
    //-+-+-+-+-+-+-+-+
    #region Methods
    public void Combine(T addData)
    {
      _processLock.Wait();
      try
      {
        addData._processLock.Wait();
        try
        {
          _Combine(addData);
          _totalIterations += addData._totalIterations;
          //Volatile.Write(ref _totalIterations, _totalIterations + addData._totalIterations);
        }
        finally
        {
          addData._processLock.Release();
        }
      }
      finally
      {
        _processLock.Release();
      }
    }

    protected abstract void _Combine(T addData);

    public void Clear()
    {
      _processLock.Wait();
      try
      {
        _Clear();
        _totalIterations = 0ul;
        //Volatile.Write(ref _totalIterations, 0ul);
      }
      finally
      {
        _processLock.Release();
      }
    }

    protected abstract void _Clear();

    public void SingleSim(CancellationToken? cancellationToken = null)
    {
      cancellationToken ??= CancellationToken.None;
      _pauseGate.Wait(cancellationToken.Value);

      _processLock.Wait();
      try
      {
        _SingleSim();
        _totalIterations++;
        //Volatile.Write(ref _totalIterations, _totalIterations + 1ul);
      }
      finally
      {
        _processLock.Release();
      }
    }

    protected abstract void _SingleSim();

    public void Dispose()
    {
      Clear();
      _Dispose();
      _pauseGate.Dispose();
      _processGate.Dispose();
      _processLock.Dispose();
    }

    protected virtual void _Dispose() { }

    public void Pause() => _pauseGate.Reset();

    public void Unpause() => _pauseGate.Set();
    #endregion
  }
}
