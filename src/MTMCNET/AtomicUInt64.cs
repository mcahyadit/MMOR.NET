using System.Threading;

namespace MMOR.NET.MTMC {
/**
 * <summary>
 *  <br/> Simple, minimal abstraction for Atomic <see cref="System.UInt64"/>.
 *  <br/> Since there are no native equivalent in NETStandard2.1.
 *  <br/>
 *  <br/> NOTE: Might consider switching to DotNext.Threading once Unity CoreCLR is stable.
 * </summary>
 */
internal sealed class AtomicUInt64 {
  private long _value;

  public AtomicUInt64(ulong initial_value = 0) => _value = (long)initial_value;

  public ulong Increment() => (ulong)Interlocked.Increment(ref _value);

  public ulong Value => (ulong)Interlocked.Read(ref _value);

  public static implicit operator AtomicUInt64(ulong x) => new(x);

  public override string ToString() => Value.ToString();
}
}
