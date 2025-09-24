using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MMOR.NET.Random;
using MMOR.NET.Utilities;

namespace MMOR.NET.MultiThreadMonteCarlo
{
  /// <summary>
  ///     The Class responsible for the Simulation Itself
  ///     ..Manages Threads, Simulation Data and handles events
  ///     ..designed to work with both Unity and Console
  ///     Does not include the Game Logic itself
  /// </summary>
  public class TestHarness
  {
    public TestHarness()
    {
      // Tells to Stop on each Exception
      // ..reduce boilerplate a little bit
      OnExceptionCatch += _ => StopTest();
    }

    //-+-+-+-+-+-+-+-+
    // Events
    // ..these are the events you can register listeners to.
    // ..this way it is detached from I/O so both Console and Unity
    // ..can implement their own formatting.
    //-+-+-+-+-+-+-+-+

    #region Events
    /// <summary>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> Use this trigger to tell the Input to not
    ///     <br /> ..accept any command or values.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> Due to the <see langword="async" /> nature of the Simulation,
    ///     <br /> ..sometimes, users might mistakenly spam the <see cref="TryRunOrStopTest" />
    ///     <br /> ..because they think the Simulation is not running.
    ///     <br /> This event alongside <see cref="OnReleaseInput" /> is used to inform
    ///     <br /> ..the I/O side that the Simulation is still in progress despite
    ///     <br /> ..not producing any output.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </summary>
    public event Action? OnHoldInput;

    /// <summary>
    ///     <br /> The Release Event for <see cref="OnHoldInput" />
    /// </summary>
    public event Action? OnReleaseInput;

    /// <summary>
    ///     <br /> Triggers once the Test has properly started.
    /// </summary>
    public event Action? OnStart;

    /// <summary>
    ///     <br /> Triggers once the Test has properly finished.
    ///     <br /> includes both success and failure.
    /// </summary>
    public event Action? OnFinish;

    /// <summary>
    ///     <br /> Triggers on any error,
    ///     <br /> Returns an <see cref="Exception" /> object.
    /// </summary>
    public event Action<Exception>? OnExceptionCatch;

    public bool currentlyTesting { get; private set; }
    //-+-+-+-+-+-+-+-+
    #endregion

    //-+-+-+-+-+-+-+-+
    // Multi-thread Logic
    //-+-+-+-+-+-+-+-+

    #region Multi-thread Logic
    // Cancelation Token is used to Break Tasks
    private CancellationTokenSource? cancellationTokenSource;
    private CancellationToken cancellationToken => cancellationTokenSource!.Token;

    /// <summary>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - <see cref="RunTest" /> and <see cref="StopTest" /> in a single method. Unity prefers this for their
    ///     Buttons.
    ///     <br /> - Equipped with try-catch encapsulation for <see cref="RunTest" />.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> <inheritdoc cref="RunTest" />
    /// </summary>
    public void TryRunOrStopTest<T>(
      ulong targetIteration,
      float checkRate,
      byte threadCount,
      IReadOnlyCollection<Func<IRandom>> rngConstructors,
      ISimSettings simSettings,
      Func<IRandom, T> simObjConstructor
    )
      where T : SimulationObject<T>
    {
      if (currentlyTesting)
        StopTest();
      else
        try
        {
          _ = RunTest(
            targetIteration,
            checkRate,
            threadCount,
            rngConstructors,
            simSettings,
            simObjConstructor
          );
        }
        catch (Exception ex)
        {
          OnExceptionCatch?.Invoke(ex);
        }
    }

    /// <summary>
    ///     <inheritdoc cref="TryRunOrStopTest" />
    /// </summary>
    public void TryRunOrStopTest<T>(
      ulong targetIteration,
      float checkRate,
      byte threadCount,
      Func<IRandom> rngConstructors,
      ISimSettings simSettings,
      Func<IRandom, T> simObjConstructor
    )
      where T : SimulationObject<T>
    {
      var rngCtors = new Queue<Func<IRandom>>();
      for (var i = 0; i < threadCount; i++)
        rngCtors.Enqueue(rngConstructors);

      TryRunOrStopTest(
        targetIteration,
        checkRate,
        threadCount,
        rngCtors,
        simSettings,
        simObjConstructor
      );
    }

    // This is the Core Logic to be ran on each Task
    private void SimulateChunk<T>(T threadData, ulong iterations)
      where T : SimulationObject<T>
    {
      for (ulong i = 0; i < iterations; i++)
      {
        // Cancellation Detected, break the loop to end the Task
        if (cancellationToken.IsCancellationRequested)
          break;

        try
        {
          threadData.SingleSim(cancellationToken);
        }
        catch (Exception ex)
        {
          OnExceptionCatch?.Invoke(ex);
        }
      }
    }

    public async Task RunTest<T>(
      ulong targetIteration,
      float checkRate,
      byte threadCount,
      IReadOnlyCollection<Func<IRandom>> rngConstructors,
      ISimSettings simSettings,
      Func<IRandom, T> simObjConstructor
    )
      where T : SimulationObject<T>
    {
      //-+-+-+-+-+-+-+-+
      // Safety Checks
      //-+-+-+-+-+-+-+-+
      #region Safety Checks
      int maxThread = Environment.ProcessorCount;
      if (threadCount < 1 || threadCount > maxThread)
      {
        var half = (byte)(maxThread / 2);
        Console.WriteLine(
          "Invalid `thread_count` argument, was {1}. Using default value `{2}`.",
          threadCount,
          half
        );
        threadCount = half;
      }
      if (rngConstructors.Count > threadCount)
        Console.WriteLine(
          "TestHarness: You have more `rng_constructors` ({0}) than `thread_count` ({1}).",
          rngConstructors.Count,
          threadCount
        );
      if (rngConstructors.Count < threadCount)
        OnExceptionCatch?.Invoke(
          new ArgumentOutOfRangeException(
            string.Format(
              "TestHarness: You have less `rng_constructors` ({0}) than `thread_count` ({1}).",
              rngConstructors.Count,
              threadCount
            )
          )
        );
      if (checkRate < 0 || checkRate > 1)
      {
        Console.WriteLine(
          "Invalid `check_rate` argument, was {0}. Using default value `0.01f`.",
          checkRate
        );
        checkRate = 0.01f;
      }
      #endregion
      //-+-+-+-+-+-+-+-+
      // Simulation Setup
      //-+-+-+-+-+-+-+-+
      #region Simulation Setup
      // Initialization
      cancellationTokenSource = new CancellationTokenSource();
      currentlyTesting = true;
      OnHoldInput?.Invoke();

      //-+-+-+-+-+-+-+-+
      // Interpret Data
      //-+-+-+-+-+-+-+-+
      threadCount = Math.Max((byte)1, threadCount); // No 0 threads, duh
      ulong checkThreshold = Math.Min(targetIteration, (ulong)(checkRate * targetIteration));
      ulong threadIterations = targetIteration / threadCount;
      ulong threadLeftOver = targetIteration % threadCount;
      Queue<Func<IRandom>> qRngCtor = !rngConstructors.IsNullOrEmpty()
        ? new Queue<Func<IRandom>>(rngConstructors)
        : new Queue<Func<IRandom>>();

      //-+-+-+-+-+-+-+-+
      // Multi-thread Setup
      //-+-+-+-+-+-+-+-+
      var tasks = new List<Task>(threadCount);
      var threadDatas = new List<T>(threadCount);

      //-+-+-+-+-+-+-+-+
      // Extra but Static Information to be printed
      // ..TODO - Grab information about CPU
      //-+-+-+-+-+-+-+-+
      List<string> seedsStr = new(threadCount);
      //-+-+-+-+-+-+-+-+

      //-+-+-+-+-+-+-+-+
      // Speed Tracking
      // ..for accuracy, we take the difference between
      // ..starting time of test and current time instead of using Time.deltaTime
      //-+-+-+-+-+-+-+-+
      Stopwatch stopWatch = new();
      stopWatch.Start();

      // This is where the Simulation in being ran,
      // ..runs on a separate thread to avoid slowing down GameController
      // ..codes below this are just progress checking
      for (byte i = 0; i < threadCount; i++)
      {
        // Set Target
        // ..the first thread will take the leftover iterations
        ulong iterations = threadIterations + (i == 0 ? threadLeftOver : 0);

        // Get RNG Constructor
        // ..fallback to SimSettings default
        if (qRngCtor.Count == 0)
          qRngCtor.Enqueue(simSettings.DefaultRNG);
        IRandom rng = qRngCtor.Dequeue().Invoke();
        // Track Seed and Algo
        seedsStr.Add(rng.ToString());

        //-+-+-+-+-+-+-+-+
        // Create, Register and Run Simulation Object
        //-+-+-+-+-+-+-+-+
        T threadData = simObjConstructor.Invoke(rng);
        threadDatas.Add(threadData);
        // Create a Pause Gate
        tasks.Add(
          Task.Factory.StartNew(
            () => SimulateChunk(threadData, iterations),
            TaskCreationOptions.LongRunning
          )
        );
      }

      // Fire Event
      OnStart?.Invoke();
      //-+-+-+-+-+-+-+-+
      #endregion
      //-+-+-+-+-+-+-+-+
      // Simulation Progress Checking
      // ..using increment and greater than comparison for more flexibility in checking
      //-+-+-+-+-+-+-+-+
      #region Simulation Progress Checking
      ulong completedIterations = 0;
      ulong nextReportThreshold = simSettings.InitialSprint ?? checkThreshold;

      // Set a Minimum for checking period
      // ..value below translate into 368 ms
      var checkPeriod = (int)Math.Ceiling(Math.Exp(-1) * 1000); // Minimum checking period.
      checkPeriod *= 5;
      var smartWait = 1000;

      //-+-+-+-+-+-+-+-+
      // Create Container for the Combined Data
      //-+-+-+-+-+-+-+-+
      T fullSimData = simObjConstructor.Invoke(null);

      while (completedIterations < targetIteration)
      {
        if (cancellationToken.IsCancellationRequested)
          break;

        //-+-+-+-+-+-+-+-+
        // Tracks current iterations
        // ..slightly roundabout with the initialization
        // ..but needs to be like this due to how the combination works
        //-+-+-+-+-+-+-+-+
        completedIterations = fullSimData.totalIterations;
        for (byte i = 0; i < threadCount; i++)
          completedIterations += threadDatas[i].totalIterations;
        // no need to lock, for just checking the totals
        //-+-+-+-+-+-+-+-+

        //-+-+-+-+-+-+-+-+
        // Need to use incremental threshold, due to
        // ..the speed and asynchronous nature of multi-threading
        // If we use modulo, it will never hit it
        if (completedIterations >= nextReportThreshold)
        {
          OnHoldInput?.Invoke();
          //-+-+-+-+-+-+-+-+
          // Combine the data from all threads
          // ..and clears it while at it
          // This will not only keeps the thread data slim,
          // ..but minimize memory duplicates as well as
          // ..making the next combine quciker
          //-+-+-+-+-+-+-+-+
          for (byte i = 0; i < threadCount; i++)
            try
            {
              T threadData = threadDatas[i];

              // Signal Thread to Pause SimulateChunk
              threadData.Pause();

              // Combine and clear
              fullSimData.Combine(threadData);
              threadData.Clear();

              // Signal Thread to continue SimulateChunk
              threadData.Unpause();
            }
            catch (Exception ex)
            {
              StopTest();
              OnExceptionCatch?.Invoke(ex);
            }

          //-+-+-+-+-+-+-+-+
          // Fire Report Events
          // ..feel free to disable or modify
          //-+-+-+-+-+-+-+-+
          try
          {
            ReportFull(
              targetIteration,
              stopWatch.Elapsed.TotalSeconds,
              seedsStr,
              fullSimData,
              simSettings.PeriodicCheckPrintsBody
            );
          }
          catch (Exception e)
          {
            OnExceptionCatch?.Invoke(e);
          }

          //-+-+-+-+-+-+-+-+
          // Setup for next Wait
          //-+-+-+-+-+-+-+-+
          // Update next Threshold
          nextReportThreshold += checkThreshold;
          // Interpolate a new wait, based on the speed
          // ..this have more overhead, but minimizes useless checking
          // ..if wait is faster than the checking period
          double speed = completedIterations / stopWatch.Elapsed.TotalSeconds;
          smartWait = Math.Min(checkPeriod, (int)Math.Ceiling(nextReportThreshold / speed * 10)); // Interpolate the wait delay based on speed

          OnReleaseInput?.Invoke();
        }

        await Task.Delay(smartWait, cancellationToken);
      }

      OnHoldInput?.Invoke();
      //-+-+-+-+-+-+-+-+
      #endregion
      //-+-+-+-+-+-+-+-+
      // Wait for Simulation to Finish
      //-+-+-+-+-+-+-+-+
      await Task.WhenAll(tasks);

      //-+-+-+-+-+-+-+-+
      // Final Report
      //-+-+-+-+-+-+-+-+
      double markTime = stopWatch.Elapsed.TotalSeconds;
      stopWatch.Stop();
      for (var i = 0; i < threadCount; i++)
        try
        {
          fullSimData.Combine(threadDatas[i]);
          threadDatas[i].Dispose();
          tasks[i].Dispose();
        }
        catch (Exception ex)
        {
          StopTest();
          OnExceptionCatch?.Invoke(ex);
        }

      currentlyTesting = false;
      ReportFull(targetIteration, markTime, seedsStr, fullSimData);
      OnReleaseInput?.Invoke();
      OnFinish?.Invoke();

      cancellationTokenSource.Dispose();
    }

    public void StopTest()
    {
      if (!currentlyTesting)
        return;
      OnHoldInput?.Invoke();
      cancellationTokenSource?.Cancel();
    }
    #endregion
    //-+-+-+-+-+-+-+-+
    // Text Handler
    //-+-+-+-+-+-+-+-+
    #region Text Handler
    /// <summary>
    ///     <br /> Invokes with Header Text, Body Text, and List of RNG Seeds
    /// </summary>
    public event Action<string, string, IReadOnlyList<string>>? OnReport;

    private void ReportFull<T>(
      in ulong targetIterations,
      in double timeElapsed,
      IReadOnlyList<string> seeds,
      T simData,
      in bool printBody = false
    )
      where T : SimulationObject<T>
    {
      string header = GenerateTextHeader(targetIterations, timeElapsed, seeds, simData);
      string body = printBody || !currentlyTesting ? GenerateTextBody(simData) : string.Empty;
      OnReport?.Invoke(header, body, seeds);
    }

    private string GenerateTextHeader<T>(
      in ulong targetIterations,
      in double timeElapsed,
      IReadOnlyList<string> seeds,
      T simData
    )
      where T : SimulationObject<T>
    {
      StringBuilder strResult = new();

      //-+-+-+-+-+-+-+-+
      // Progress Information
      //-+-+-+-+-+-+-+-+
      ulong currentIterations = simData.totalIterations;
      double averageSpeed = currentIterations / timeElapsed;
      double estimatedTime = (targetIterations - currentIterations) / averageSpeed;
      float completionPercentage = (float)currentIterations / targetIterations;

      if (currentlyTesting)
      {
        strResult
          .Append("Current ")
          .AppendFormat("{0:N0}", currentIterations)
          .Append(" (")
          .Append(completionPercentage.ToPercentage())
          .AppendLine(")");

        // There was some error when
        // ..averageSpeed == 0
        // .. do a check, just to prevent throw
        if (averageSpeed > 0 && targetIterations - currentIterations > 0)
        {
          strResult
            .Append("Current Speed: ")
            .AppendFormat("{0:N2}", averageSpeed)
            .Append("/s")
            .Append(" | ")
            .Append("Est. time remaining: ")
            .Append(estimatedTime.ToTime())
            .Append(" | ")
            .Append("Time Elapsed: ")
            .AppendLine(timeElapsed.ToTime());
        }
      }
      else
      {
        if (currentIterations >= targetIterations)
          strResult.Append("Completed ").AppendFormat("{0:N0}", currentIterations).AppendLine();
        else
          strResult
            .Append("Aborted After ")
            .AppendFormat("{0:N0}", currentIterations)
            .Append(" (")
            .Append(completionPercentage.ToPercentage())
            .AppendLine(")");

        strResult
          .Append("Average Speed: ")
          .AppendFormat("{0:N2}", averageSpeed)
          .Append("/s")
          .Append(" | ")
          .Append("Completed in ")
          .AppendLine(timeElapsed.ToTime());
      }

      strResult.AppendLine();
      strResult.AppendLine("PRNG Algo-Seed(in Hexadecimal):");
      int len = seeds.Count;
      for (var i = 0; i < len; i++)
      {
        if (i > 0)
          strResult.Append(", ");
        strResult.Append(seeds[i]);
      }

      strResult.AppendLine();
      strResult.AppendLine();

      strResult.AppendLine(simData.PrettyPrintHeader());

      return strResult.ToString();
    }

    private static string GenerateTextBody<T>(T simData)
      where T : SimulationObject<T>
    {
      return simData.PrettyPrintBody();
    }
    #endregion
  }
}
