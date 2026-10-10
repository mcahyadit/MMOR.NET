using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MMOR.NET.Collections {
public partial class RentedListTest {
  public struct RandomizedMixParam {
    public string identifier { get; init; } = "Generic Test";
    public int seed { get; init; }
    public int trials { get; init; }
    public int steps { get; init; }
    public RandomizedMixParam() {}
    public override string ToString() => identifier;
  }

  public enum SampleEnum { A, B, C, D }

  public struct SampleStruct {
    public int X;
    public SampleStruct(int x) {
      X = x;
    }
  }

  [Theory]
  [MemberData(nameof(kRandomizedMixParams))]
  public void RandomizedOperationMix_MatchesListOracle(RandomizedMixParam p) {
    System.Random rng = new(p.seed);
    for (int trial = 0; trial < p.trials; ++trial) {
      using RentedList<int> rented = new();
      List<int> oracle             = new();
      for (int step = 0; step < p.steps; ++step) {
        switch (rng.Next(4)) {
          case 0: {
            int v = rng.Next(-100, 100);
            rented.Add(v);
            oracle.Add(v);
            break;
          }
          case 1: {
            int idx = rng.Next(oracle.Count + 1);
            int v   = rng.Next(-100, 100);
            rented.Insert(idx, v);
            oracle.Insert(idx, v);
            break;
          }
          case 2: {
            if (oracle.Count == 0)
              break;
            int idx = rng.Next(oracle.Count);
            rented.RemoveAt(idx);
            oracle.RemoveAt(idx);
            break;
          }
          default: {
            int v = rng.Next(-100, 100);
            Assert.Equal(oracle.Remove(v), rented.Remove(v));
            break;
          }
        }
        Assert.Equal(oracle.Count, rented.Count);
        Assert.Equal(oracle, rented.AsSpan().ToArray());
      }
    }
  }

  [Fact]
  public void Constructor_InitialValueCount_FillsRequestedCount() {
    using RentedList<int> list = new(7, 5);
    Assert.Equal(5, list.Count);
    Assert.Equal(new[] { 7, 7, 7, 7, 7 }, list.AsSpan().ToArray());
  }

  [Fact]
  public void Constructor_Span_CopiesContents() {
    using RentedList<int> list = new((ReadOnlySpan<int>)new[] { 1, 2, 3, 4 });
    Assert.Equal(4, list.Count);
    Assert.Equal(new[] { 1, 2, 3, 4 }, list.AsSpan().ToArray());
  }

  [Fact]
  public void Constructor_Capacity_LeavesCountZero() {
    using RentedList<int> list = new(100);
    Assert.Empty(list);
    Assert.True(list.Capacity >= 100);
  }

  [Fact]
  public void Add_GrowsPastPoolMinimum_PreservesOrder() {
    using RentedList<int> list = new();
    for (int i = 0; i < 1000; ++i) list.Add(i);
    Assert.Equal(1000, list.Count);
    Assert.Equal(Enumerable.Range(0, 1000).ToArray(), list.AsSpan().ToArray());
  }

  [Fact]
  public void AddRange_Span_Appends() {
    using RentedList<int> list = new();
    list.AddRange((ReadOnlySpan<int>)new[] { 1, 2, 3, 4, 5 });
    Assert.Equal(5, list.Count);
    Assert.Equal(new[] { 1, 2, 3, 4, 5 }, list.AsSpan().ToArray());
    list.AddRange((ReadOnlySpan<int>)new[] { 6, 7 });
    Assert.Equal(new[] { 1, 2, 3, 4, 5, 6, 7 }, list.AsSpan().ToArray());
  }

  [Fact]
  public void AddRange_Collection_Appends() {
    using RentedList<int> list = new();
    list.AddRange((ICollection<int>)new List<int> { 10, 20 });
    list.AddRange((ICollection<int>)new List<int> { 30 });
    Assert.Equal(new[] { 10, 20, 30 }, list.AsSpan().ToArray());
  }

  [Fact]
  public void Indexer_IntAndIndex_GetSet() {
    using RentedList<int> list = new();
    list.AddRange((ReadOnlySpan<int>)new[] { 10, 20, 30 });

    Assert.Equal(20, list[1]);
    list[1] = 99;
    Assert.Equal(99, list[1]);

    Assert.Equal(30, list[^1]);
    list[^1] = 42;
    Assert.Equal(42, list[2]);
  }

  [Fact]
  public void CopyTo_HonorsArrayIndex() {
    using RentedList<int> list = new();
    list.AddRange((ReadOnlySpan<int>)new[] { 1, 2, 3 });

    int[] dst = new int[5];
    list.CopyTo(dst, 2);
    Assert.Equal(new[] { 0, 0, 1, 2, 3 }, dst);
  }

  [Fact]
  public void IndexOf_Int_MatchesLinearScan() {
    System.Random rng = new(7);
    for (int n = 0; n <= 200; ++n) {
      int[] data = new int[n];
      for (int i = 0; i < n; ++i) data[i] = rng.Next(-5, 5);

      using RentedList<int> list = new();
      list.AddRange((ReadOnlySpan<int>)data);

      int probe    = rng.Next(-5, 5);
      int expected = Array.IndexOf(data, probe);
      Assert.Equal(expected, list.IndexOf(probe));
      Assert.Equal(expected >= 0, list.Contains(probe));
    }
  }

  [Fact]
  public void IndexOf_Char_MatchesLinearScan() {
    System.Random rng = new(11);
    char[] data       = new char[100];
    for (int i = 0; i < data.Length; ++i) data[i] = (char)('a' + rng.Next(0, 5));

    using RentedList<char> list = new();
    list.AddRange((ReadOnlySpan<char>)data);

    for (char probe = 'a'; probe <= 'g'; ++probe) {
      Assert.Equal(Array.IndexOf(data, probe), list.IndexOf(probe));
      Assert.Equal(Array.IndexOf(data, probe) >= 0, list.Contains(probe));
    }
  }

  [Fact]
  public void IndexOf_Bool_MatchesLinearScan() {
    System.Random rng = new(13);
    bool[] data       = new bool[100];
    for (int i = 0; i < data.Length; ++i) data[i] = rng.Next(2) == 0;

    using RentedList<bool> list = new();
    list.AddRange((ReadOnlySpan<bool>)data);

    Assert.Equal(Array.IndexOf(data, true), list.IndexOf(true));
    Assert.Equal(Array.IndexOf(data, false), list.IndexOf(false));
  }

  [Fact]
  public void IndexOf_DoubleNaN_FindsNaN() {
    using RentedList<double> list = new();
    list.AddRange((ReadOnlySpan<double>)kDoubleValues);

    Assert.Equal(42, list.IndexOf(double.NaN));
    Assert.Contains(double.NaN, list);
    Assert.Equal(-1, list.IndexOf(12345.0));
  }

  [Fact]
  public void IndexOf_Enum_MatchesEquality() {
    using RentedList<SampleEnum> list = new();
    list.AddRange((ReadOnlySpan<SampleEnum>)kEnumValues);

    Assert.Equal(1, list.IndexOf(SampleEnum.B));
    Assert.Equal(-1, list.IndexOf((SampleEnum)99));
    Assert.Contains(SampleEnum.D, list);
  }

  [Fact]
  public void IndexOf_Struct_MatchesEquality() {
    using RentedList<SampleStruct> list = new();
    list.AddRange((ReadOnlySpan<SampleStruct>)kStructValues);

    Assert.Equal(1, list.IndexOf(new SampleStruct(2)));
    Assert.Equal(-1, list.IndexOf(new SampleStruct(9)));
  }

  [Fact]
  public void EnsureCapacity_Grows_AndNeverShrinks() {
    using RentedList<int> list = new();
    list.AddRange((ReadOnlySpan<int>)new[] { 1, 2, 3 });

    int cap = list.Capacity;
    list.EnsureCapacity(2);
    Assert.Equal(cap, list.Capacity);
    Assert.Equal(new[] { 1, 2, 3 }, list.AsSpan().ToArray());

    list.EnsureCapacity(500);
    Assert.True(list.Capacity >= 500);
    Assert.Equal(new[] { 1, 2, 3 }, list.AsSpan().ToArray());
  }

  [Fact]
  public void GrowCapacity_ExceedsCurrent() {
    using RentedList<int> list = new();
    Assert.True(list.GrowCapacity() > list.Capacity);
  }

  [Fact]
  public void Insert_OutOfRange_Throws() {
    using RentedList<int> list = new();
    list.Add(1);
    Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(2, 9));
    Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(-1, 9));
  }

  [Fact]
  public void RemoveAt_OutOfRange_Throws() {
    using RentedList<int> list = new();
    list.Add(1);
    Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(1));
    Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(-1));
  }

  [Fact]
  public void Clear_ResetsCount() {
    using RentedList<int> list = new();
    list.AddRange((ReadOnlySpan<int>)new[] { 1, 2, 3 });
    list.Clear();
    Assert.Empty(list);
    Assert.True(list.AsSpan().IsEmpty);
    list.Add(5);
    Assert.Equal(new[] { 5 }, list.AsSpan().ToArray());
  }

  [Fact]
  public void Enumerator_YieldsAllInOrder() {
    using RentedList<int> list = new();
    list.AddRange((ReadOnlySpan<int>)new[] { 1, 2, 3, 4, 5 });

    int[] expected = { 1, 2, 3, 4, 5 };
    int i          = 0;
    foreach (int v in list) Assert.Equal(expected[i++], v);
    Assert.Equal(5, i);
  }

  [Fact]
  public void Enumerator_ThrowsOnModification() {
    using RentedList<int> list = new();
    list.AddRange((ReadOnlySpan<int>)new[] { 1, 2, 3 });

    Assert.Throws<InvalidOperationException>(() => {
      foreach (int _ in list) list.Add(99);
    });
  }

  [Fact]
  public void Dispose_IsIdempotent() {
    RentedList<int> list = new();
    list.Add(1);
    list.Dispose();
    list.Dispose();
  }
}
}
