using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MMOR.Utils.Random
{
    public abstract class vRandom<T> : vRandom where T : vRandom, new()
    {
        private static T Global;
        public static T global => Global ??= new T();
    }

    /// <summary>
    ///     <strong>abstract vRandom</strong>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Abstract for RNG Algorithms.
    ///     <br /> - Allows easy switching.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </summary>
    public abstract class vRandom
    {
        protected ulong seed;

        protected virtual void Initialize(ulong seed, params ulong[] additonalParameters) { this.seed = seed; }

        protected static ulong DefaultReSeed()
        {
            var seed = (ulong)(Environment.TickCount ^ Thread.CurrentThread.ManagedThreadId ^
                               Guid.NewGuid().GetHashCode());
            seed = (seed << 32) | seed;
            return (uint)seed;
        }

        public abstract string getSeed();
        public abstract uint NextUInt(); // Primary Result

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (minInclusive >= maxExclusive)
                throw new Exception($"maxExclusive {maxExclusive} must be greater than minInclusive {minInclusive}");
            return (int)(minInclusive + NextUInt() % (uint)(maxExclusive - minInclusive));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat(float minInclusive = 0, float maxExclusive = 1)
        {
            if (minInclusive >= maxExclusive)
                throw new Exception($"maxExclusive {maxExclusive} must be greater than minInclusive {minInclusive}");
            return minInclusive + (float)NextUInt() / uint.MaxValue * (maxExclusive - minInclusive);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double NextDouble(double minInclusive = 0, double maxExclusive = 1)
        {
            if (minInclusive >= maxExclusive)
                throw new Exception($"maxExclusive {maxExclusive} must be greater than minInclusive {minInclusive}");
            return minInclusive + (double)NextUInt() / uint.MaxValue * (maxExclusive - minInclusive);
        }
    }

    public class NETdefault : vRandom<NETdefault>
    {
        private System.Random random;

        public NETdefault() : this(DefaultReSeed()) { }

        public NETdefault(ulong seed, params ulong[] additonalParameters) { Initialize(seed, additonalParameters); }

        public override string getSeed() { return $".NET-0x{seed:X}"; }

        protected override void Initialize(ulong seed, params ulong[] additonalParameters)
        {
            base.Initialize(seed, additonalParameters);

            var castedSeed = (int)seed;
            this.seed = (ulong)castedSeed;
            random = new System.Random(castedSeed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint NextUInt() { return (uint)random.Next(); }
    }
}