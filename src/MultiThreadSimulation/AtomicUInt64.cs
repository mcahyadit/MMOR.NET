using System.Threading;

namespace MMOR.NET.MultiThreadSimulation {
/**
 * <summary>
 *  <br/> Simple, minimal abstraction for Atomic <see cref="System.UInt64"/>.
 *  <br/> Since there are no native equivalent in NETStandard2.1.
 *  <br/>
 *  <br/> NOTE: Might consider switching to DotNext.Threading once Unity CoreCLR is stable.
 * </summary>
 */
public sealed class AtomicUInt64 {
  // Since `Interlocked.Increment` doesn't work with `ulong`,
  // .. we rely on `long` as the backing data and converts it
  // .. along with any data overflow.

  private long value_;

  public AtomicUInt64(ulong initial_value = 0) => value_ = (long)initial_value;

  public ulong Increment() {
    ulong initial, computed;
    do {
      initial  = (ulong)Interlocked.Read(ref value_);  // re-read fresh value each retry
      computed = initial + 1;
    } while (
        // Write to value_, if the local increment matches the result.
        // .. on false, another thread modified it so retry the increment
        Interlocked.CompareExchange(ref value_, (long)computed, (long)initial) != (long)initial);
    return computed;
  }

  public ulong Value => (ulong)Interlocked.Read(ref value_);

  public static implicit operator AtomicUInt64(ulong x) => new(x);

  public override string ToString() => Value.ToString();
}
}
