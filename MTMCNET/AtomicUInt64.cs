using System.Threading;

namespace MMOR.NET.MTMC {
  /// <summary>
  /// There was no native Atomic<uint64> in NET Standard 2.1
  /// </summary>
  internal class atomic_uint64_t {
    private long _value;

    public atomic_uint64_t(ulong initial_value = 0) => _value = (long)initial_value;

    public ulong Increment() => (ulong)Interlocked.Increment(ref _value);

    public ulong Value => (ulong)Interlocked.Read(ref _value);

    public static implicit operator ulong(atomic_uint64_t x) => x.Value;

    public static implicit operator atomic_uint64_t(ulong x) => new(x);

    public override string ToString() => _value.ToString();
  }
}
