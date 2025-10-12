using System;
using System.Collections.Generic;
using MMOR.NET.Random;

namespace MMOR.NET.MTMC {
  public abstract class SimulationConfig<T>
      where T : SimulationObject<T> {
    public Func<IRandom, T> sim_obj_ctor;
    public ulong target_iteration = 1_000_000_000;
    public float check_rate = 0.01f;
    public ushort thread_count = (ushort)(Environment.ProcessorCount / 2);
    public List<Func<IRandom>> rng_ctor = null;
    public bool periodic_check_prints_body = false;
    public TimeSpan minimum_wait = TimeSpan.FromMilliseconds(2368);
    public TimeSpan maximum_wait = TimeSpan.FromSeconds(60);
    public uint? initial_sprint = null;
  }
}
