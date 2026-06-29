# About

A set of utilities for running multi-threaded simulation easier.

Depends on `MMOR.NET.Random`.

## TestHarness

This class would be the orchestrator / controller of the Simulation. **TestHarness** is responsible
for creation and assignment of [**ISimulationObject**](#isimulationobject) for each threads, dealing with
the thread-safety concerns and periodically triggers a report.

### OnReport Event

This event will periodically send a _reference_ to an copy of **ISimulationObject** that contains
the combined data from all threads so far.

This is set to be asynchronous compatible in case the reporting require it. When doing a
synchronous process, do return a `Task.Completed` just to tell the TestHarness that it is ready for
the next reporting.

Since this is a _reference_, it is _possible_ for you to modify the data during the reporting. You might
want to do this if you say want to store this simulated data into an SQL database and then calling
`Clear` afterwards to allow clean input to the database on the next invocation.

Such behaviour would not cause issue in the simulation as this **reference** is to a separate copy
unrelated to the ones running in each thread and only acts as an accumulator of sorts.

### Report Poking

Poking would signal the **TestHarness** to basically trigger **OnReport** on the next available moment.
It is designed to not interrupt any on-going process and instead signals to pause the threads from
starting a new one.

## ISimulationObject

This interface defines sets of calls the Simulator needs.

### Simulation Metadata

The first amongst them is the `#!cs SimulationMetadata sim_meta`. In exchange using the more flexible
`#!cs interface` instead of the more restrictive `#!cs abstract class`, this is a single component that
contains the data required by the Simulator. For the most part you just need to call a simple
`#!cs sim_meta = new()` during the class' constructor.

> [!WARNING]
>
> All the data inside are **publicly** accessible on purpose for library consumers who would like more
> control. Do so at your own risk of making the Simulation non-deterministic.

Among these metadata, are the same mutex locks that the TestHarness uses, so it is possible while
not recommended to slip in extra operations.

### Processes with Builtin Interlocking

The next few things to implement are:

- `SingleSim`
- `Combine`
- `Clear`

Each of these are what define what you are Simulating.

Say you tell [**TestHarness**](#testharness) to run 1000 times, `SingleSim` is the function that will
be called 1000 times.

`Combine` will be the function that defines how data from two of the SimulationObject can be
combined. While `Clear` needs to define how the data are reset.

For more details on how to implement, refer to the [examples](./02-examples.md).
