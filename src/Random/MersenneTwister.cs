using System.Runtime.CompilerServices;

namespace MMOR.Utils.Random
{
    /// <summary>
    ///     <strong><u>M</u>ersenne <u>T</u>wister 19937</strong>
    ///     <br />
    ///     -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Same algo with C++ GeneralLibrary, implemented in C#.
    ///     <br /> - <b>19937</b> indicates the length. Meaning the RNG will exhaust its randomness in 2 ^ 19937 - 1
    ///     iterations.
    ///     <br /> - Automatically reseed when range is exhausted.
    ///     <br /> - Implements generic abstract class with other rng for interchangibility.
    ///     <br />
    ///     -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </summary>
    public class MT19937 : IRandom<MT19937>
    {
        protected const int N = 624;
        protected const int M = 397;
        protected const uint matrixA = 0x9908b0dfU;
        protected const uint upperMask = 0x80000000U;
        protected const uint lowerMask = 0x7fffffffU;
        protected readonly uint[] mtVal = new uint[N];
        protected int mtDex = N + 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint NextUInt()
        {
            if (mtDex >= N)
            {
                //if (mtDex == N + 1)
                //    Initialize(DefaultReSeed());

                for (var k = 0; k < N - M; k++)
                {
                    uint y = (mtVal[k] & upperMask) | (mtVal[k + 1] & lowerMask);
                    mtVal[k] = mtVal[k + M] ^ (y >> 1) ^ ((y & 0x1) * matrixA);
                }

                for (int k = N - M; k < N - 1; k++)
                {
                    uint y = (mtVal[k] & upperMask) | (mtVal[k + 1] & lowerMask);
                    mtVal[k] = mtVal[k + (M - N)] ^ (y >> 1) ^ ((y & 0x1) * matrixA);
                }

                uint lastY = (mtVal[N - 1] & upperMask) | (mtVal[0] & lowerMask);
                mtVal[N - 1] = mtVal[M - 1] ^ (lastY >> 1) ^ ((lastY & 0x1) * matrixA);

                mtDex = 0;
            }

            uint number = mtVal[mtDex++];

            // Tempering
            number ^= number >> 11;
            number ^= (number << 7) & 0x9d2c5680;
            number ^= (number << 15) & 0xefc60000;
            number ^= number >> 18;

            return number;
        }

        //-+-+-+-+-+-+-+-+
        // Initialization
        //-+-+-+-+-+-+-+-+

        #region Initialization
        public MT19937() : this(DefaultReSeed()) { }

        public MT19937(ulong seed)
        {
            Seed = seed;
            mtVal[0] = (uint)seed;
            for (mtDex = 1; mtDex < N; mtDex++)
                mtVal[mtDex] = 1812433253u * (mtVal[mtDex - 1] ^ (mtVal[mtDex - 1] >> 30)) + (uint)mtDex;
        }

        public override string ToString() => $"MT19937-0x{Seed:X}";

        //-+-+-+-+-+-+-+-+
        #endregion
    }
}