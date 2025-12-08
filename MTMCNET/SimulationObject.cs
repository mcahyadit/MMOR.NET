using System.Threading;
using MMOR.NET.RichString;

namespace MMOR.NET.MTMC {
  public abstract class SimulationObject<T>
      where T : SimulationObject<T> {
    //-+-+-+-+-+-+-+-+
    // Generic Data
    //-+-+-+-+-+-+-+-+
    internal string? kRngIdentifier { get; set; }
    public ulong total_iterations { get; private set; }
    private readonly ManualResetEventSlim pause_gate_ = new(true);
    private readonly SemaphoreSlim process_lock_ = new(1, 1);

    //-+-+-+-+-+-+-+-+
    // Pretty Print
    //-+-+-+-+-+-+-+-+
    public abstract IRichString PrettyPrintHeader();
    public abstract IRichString PrettyPrintBody();

//-+-+-+-+-+-+-+-+
// Methods
//-+-+-+-+-+-+-+-+
#region Methods
    public void Combine_(T addData) {
      process_lock_.Wait();
      try {
        addData.process_lock_.Wait();
        try {
          Combine(addData);
          total_iterations += addData.total_iterations;
        } finally {
          addData.process_lock_.Release();
        }
      } finally {
        process_lock_.Release();
      }
    }

    protected abstract void Combine(T addData);

    internal void Clear_() {
      process_lock_.Wait();
      try {
        Clear();
        total_iterations = 0ul;
      } finally {
        process_lock_.Release();
      }
    }

    protected abstract void Clear();

    internal void SingleSim_(CancellationToken? cancellationToken = null) {
      cancellationToken ??= CancellationToken.None;
      pause_gate_.Wait(cancellationToken.Value);

      process_lock_.Wait();
      try {
        SingleSim();
        total_iterations++;
      } finally {
        process_lock_.Release();
      }
    }

    protected abstract void SingleSim();

    internal void Dispose_() {
      Clear();
      Dispose();
      pause_gate_.Dispose();
      process_lock_.Dispose();
    }

    protected virtual void Dispose() {}

    internal void Pause() => pause_gate_.Reset();

    internal void Unpause() => pause_gate_.Set();
#endregion
  }
}
