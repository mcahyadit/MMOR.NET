using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MMOR.NET.Collections {
public partial class RentedBoolListTest {
  public struct RandomizedMixParam {
    public string identifier { get; init; } = "Generic Test";
    public int seed { get; init; }
    public int trials { get; init; }
    public int steps { get; init; }
    public RandomizedMixParam() {}
    public override string ToString() => identifier;
  }

  static bool[] Snapshot(RentedBoolList list) {
    bool[] result = new bool[list.Count];
    for (int i = 0; i < list.Count; ++i) result[i] = list[i];
    return result;
  }

  [Theory]
  [MemberData(nameof(kRandomizedMixParams))]
  public void RandomizedOperationMix_MatchesListOracle(RandomizedMixParam p) {
    System.Random rng = new(p.seed);
    for (int trial = 0; trial < p.trials; ++trial) {
      using RentedBoolList list = new();
      List<bool> oracle         = new();
      for (int step = 0; step < p.steps; ++step) {
        switch (rng.Next(4)) {
          case 0: {
            bool v = rng.Next(2) == 0;
            list.Add(v);
            oracle.Add(v);
            break;
          }
          case 1: {
            int idx = rng.Next(oracle.Count + 1);
            bool v  = rng.Next(2) == 0;
            list.Insert(idx, v);
            oracle.Insert(idx, v);
            break;
          }
          case 2: {
            if (oracle.Count == 0)
              break;
            int idx = rng.Next(oracle.Count);
            list.RemoveAt(idx);
            oracle.RemoveAt(idx);
            break;
          }
          default: {
            bool v = rng.Next(2) == 0;
            Assert.Equal(oracle.Remove(v), list.Remove(v));
            break;
          }
        }
        Assert.Equal(oracle.Count, list.Count);
        Assert.Equal(oracle.ToArray(), Snapshot(list));
      }
    }
  }

  [Fact]
  public void Indexer_SetHighBitAcrossWords() {
    using RentedBoolList list = new();
    const int n               = 200;
    for (int i = 0; i < n; ++i) list.Add(false);

    list[63]  = true;
    list[64]  = true;
    list[127] = true;
    list[128] = true;
    list[199] = true;

    for (int i = 0; i < n; ++i) {
      bool expected = i == 63 || i == 64 || i == 127 || i == 128 || i == 199;
      Assert.Equal(expected, list[i]);
    }
  }

  [Theory]
  [MemberData(nameof(kIndexOfLengths))]
  public void IndexOf_MatchesOracle(int n) {
    System.Random rng         = new(n * 31 + 7);
    bool[] data               = new bool[n];
    using RentedBoolList list = new();
    for (int i = 0; i < n; ++i) {
      bool v  = rng.Next(2) == 0;
      data[i] = v;
      list.Add(v);
    }

    Assert.Equal(Array.IndexOf(data, true), list.IndexOf(true));
    Assert.Equal(Array.IndexOf(data, false), list.IndexOf(false));
    Assert.Equal(Array.IndexOf(data, true) >= 0, list.Contains(true));
    Assert.Equal(Array.IndexOf(data, false) >= 0, list.Contains(false));
  }

  [Theory]
  [MemberData(nameof(kLastPartialWordLengths))]
  public void IndexOf_FindsLastPartialWordBit(int n) {
    bool[] data = new bool[n];
    for (int i = 0; i < n; ++i) data[i] = i == n - 1;

    using RentedBoolList list = new();
    for (int i = 0; i < n; ++i) list.Add(data[i]);

    Assert.Equal(Array.IndexOf(data, true), list.IndexOf(true));
    Assert.Equal(Array.IndexOf(data, false), list.IndexOf(false));
  }

  [Theory]
  [MemberData(nameof(kInsertParams))]
  public void Insert_MatchesOracle(int index, bool item) {
    System.Random rng         = new(1234);
    List<bool> oracle         = new();
    using RentedBoolList list = new();
    for (int i = 0; i < 130; ++i) {
      bool v = rng.Next(2) == 0;
      oracle.Add(v);
      list.Add(v);
    }

    oracle.Insert(index, item);
    list.Insert(index, item);

    Assert.Equal(oracle.Count, list.Count);
    Assert.Equal(oracle.ToArray(), Snapshot(list));
  }

  [Fact]
  public void Insert_OutOfRange_Throws() {
    using RentedBoolList list = new();
    list.Add(true);
    Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(2, true));
    Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(-1, true));
  }

  [Theory]
  [MemberData(nameof(kRemoveAtIndexes))]
  public void RemoveAt_MatchesOracle(int index) {
    System.Random rng         = new(4321);
    List<bool> oracle         = new();
    using RentedBoolList list = new();
    for (int i = 0; i < 130; ++i) {
      bool v = rng.Next(2) == 0;
      oracle.Add(v);
      list.Add(v);
    }

    oracle.RemoveAt(index);
    list.RemoveAt(index);

    Assert.Equal(oracle.Count, list.Count);
    Assert.Equal(oracle.ToArray(), Snapshot(list));
  }

  [Fact]
  public void RemoveAt_OutOfRange_Throws() {
    using RentedBoolList list = new();
    list.Add(true);
    Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(1));
    Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(-1));
  }

  [Fact]
  public void AsSpan_WordCountCeils() {
    using RentedBoolList empty = new();
    Assert.Equal(0, empty.AsSpan().Length);

    using RentedBoolList list = new();
    for (int i = 0; i < 64; ++i) list.Add(false);
    Assert.Equal(1, list.AsSpan().Length);
    list.Add(false);
    Assert.Equal(2, list.AsSpan().Length);
  }

  [Fact]
  public void EnsureCapacity_Grows_AndNeverShrinks() {
    using RentedBoolList list = new();
    list.Add(true);

    int cap = list.Capacity;
    list.EnsureCapacity(1);
    Assert.Equal(cap, list.Capacity);
    Assert.True(list[0]);

    list.EnsureCapacity(4096);
    Assert.True(list.Capacity >= 4096);
    Assert.True(list[0]);
  }

  [Fact]
  public void Clear_ResetsCount() {
    using RentedBoolList list = new();
    for (int i = 0; i < 100; ++i) list.Add(i % 2 == 0);

    list.Clear();
    Assert.Empty(list);

    list.Add(true);
    Assert.Single(list);
    Assert.True(list[0]);
  }

  [Fact]
  public void CopyTo_HonorsArrayIndex() {
    using RentedBoolList list = new();
    for (int i = 0; i < 70; ++i) list.Add(i % 2 == 0);

    bool[] dst = new bool[80];
    list.CopyTo(dst, 5);

    for (int i = 0; i < 5; ++i) Assert.False(dst[i]);
    for (int i = 0; i < 70; ++i) Assert.Equal(i % 2 == 0, dst[5 + i]);
  }

  [Fact]
  public void CopyTo_TooSmall_Throws() {
    using RentedBoolList list = new();
    for (int i = 0; i < 10; ++i) list.Add(true);
    Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(new bool[5], 0));
  }

  [Fact]
  public void Enumerator_YieldsAllInOrder() {
    using RentedBoolList list = new();
    bool[] expected           = new bool[130];
    System.Random rng         = new(5);
    for (int i = 0; i < expected.Length; ++i) {
      expected[i] = rng.Next(2) == 0;
      list.Add(expected[i]);
    }

    int idx = 0;
    foreach (bool v in list) Assert.Equal(expected[idx++], v);
    Assert.Equal(expected.Length, idx);
  }

  [Fact]
  public void Enumerator_ThrowsOnModification() {
    using RentedBoolList list = new();
    list.Add(true);
    list.Add(false);
    list.Add(true);

    Assert.Throws<InvalidOperationException>(() => {
      foreach (bool _ in list) list.Add(true);
    });
  }

  [Fact]
  public void Dispose_IsIdempotent() {
    RentedBoolList list = new();
    list.Add(true);
    list.Dispose();
    list.Dispose();
  }
}
}
