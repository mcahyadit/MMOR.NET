using System;
using MMOR.NET.Random;

namespace MMOR.NET.MultiThreadMonteCarlo
{
  public interface ISimSettings
  {
    /// <summary>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Defines the first few iterations..
    ///     <br /> - where the checking delay is ignored.
    ///     <br /> - Useful for confirmation whether the..
    ///     <br /> - the Simulation is running or not.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Set to <see langword="null" /> to disable.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </summary>
    public uint? InitialSprint { get; }

    /// <summary>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Defines the default constructor for..
    ///     <br /> - the RNG should it be undefined, or..
    ///     <br /> - the defined are less than the threadCount.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </summary>
    public Func<IRandom> DefaultRNG { get; }

    /// <summary>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Tells the TestHarness whether to..
    ///     <br /> - ..call the PrettyPrint for the..
    ///     <br /> - ..Report Body or not.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Reporting the Body slightly takes more..
    ///     <br /> - ..computational power, and it's length..
    ///     <br /> - ..can cause issue when printing in Console.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </summary>
    public bool PeriodicCheckPrintsBody { get; }
  }
}
