# About

`MTMC` which stands for _**M**ulti-**T**hreaded **M**onte-**C**arlo_, is a set of utilities I developed for
doing a long-running simulation with abstractions over the multi-threaded orchestration.

### Components

- SimulationObject
- TestHarness
- SimulationConfig

## SimulationObject

This will be the core object that is simulated. Containing the simulated data and the methods of:

- `SingleSim`
- `Combine`
- `Clear`

The parent class mostly just adds in a _thread-safe_ wrapper over these three functions when it will
be called by [`TestHarness`](#testharness).

### Example

Simulating consecutive 50/50 up to 100.

```cs
using MMOR.NET.MTMC;
using MMOR.NET.Statistics;

public class SomeSimulation : SimulationObject<SomeSimulation> {
  IRandom rng_;
  RunningStatistics stats;

  public SomeSimulation(IRandom rng) {
    rng_ = rng;
    stats = new();
  }

  public override void SingleSim() {
    int consecutives = 0;
    while (rng_.NextUInt() < (uint.MaxValue >> 1)) {
      if (++consecutives == 100)
        break;
    }
    stats.Push(consecutives);
  }

  public override void Combine(SomeSimulation add_data) {
    stats.Push(add_data.stats);
  }

  public override void Clear() {
    stats.Clear();
  }
}
```

## TestHarness

### Diagram

```mermaid
---
config:
  layout: elk
---
flowchart TD
  Start["TestHarness::RunTest"] --> Spawn
  Spawn@{ shape: processes, label: "Spawn × N Workers" }
  Spawn --> Run
  Spawn -.-> WStart

  subgraph Controller["Controller (RunTest)"]
    Run@{ shape: diamond, label: "Progress<br/>Sufficient?" }
    Run -->|No| Pause["pause_gate.Reset"]
    Pause --> Collect["Combine +<br/>Clear"]
    Collect --> Unpause["pause_gate.Set"]
    Unpause --> Wait@{ shape: delay, label: "Wait" }
    Wait --> Report@{ shape: trap-b, label: "Report" }
    Report --> Run
    Run -->|Yes| WaitAll["Task.WhenAll"]
  end

  subgraph Worker["Worker Thread × N"]
    WStart@{ shape: circle, label: "start" } --> WLoop
    WLoop@{ shape: diamond, label: "Iterations<br/>Remain?" }
    WLoop -->|Yes| WGate@{ shape: delay, label: "pause_gate.Wait" }
    WGate --> WSim["SingleSim"]
    WSim --> WLoop
    WLoop -->|No| WFinish@{ shape: stadium, label: "Finish" }
  end

  Pause -.->|Blocks| WGate
  Unpause -.->|Unblocks| WGate
  Collect -.->|process_lock_ sync| WSim
  WFinish --> WaitAll
  WaitAll --> Final["Final Combine + Dispose"]
  Final --> Done@{ shape: stadium, label: "Done" }

```
