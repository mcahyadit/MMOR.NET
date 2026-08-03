using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace MMOR.NET.Benchmarks {
public static class Program {
  public static void Main(string[] args) {
    ManualConfig config = DefaultConfig
                              .Instance  //
                              .AddJob(Job.Default.WithRuntime(CoreRuntime.Core10_0))
                              .AddJob(Job.Default.WithRuntime(NativeAotRuntime.Net10_0))
                              .AddJob(Job.Default.WithRuntime(CoreRuntime.Core80))
                              .AddJob(Job.Default.WithRuntime(NativeAotRuntime.Net80));
    BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
  }
}
}
