using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MMOR.NET.Utilities {

public static class MultiStdErr {
  private static readonly Lazy<MultiTextWriter> singleton_ = new(() => {
    MultiTextWriter instance = new();
    TextWriter original      = Console.Error;
    _                        = instance.Register(original);
    Console.SetError(instance);
    return instance;
  });

  /// <inheritdoc cref="MultiTextWriter.Register(TextWriter)"/>
  public static Guid Register(TextWriter w) {
    return singleton_.Value.Register(w);
  }

  /// <inheritdoc cref="MultiTextWriter.Unregister(Guid)"/>
  public static bool Unregister(Guid guid) {
    return singleton_.Value.Unregister(guid);
  }
}

public static class MultiStdOut {
  private static readonly Lazy<MultiTextWriter> singleton_ = new(() => {
    MultiTextWriter instance = new();
    TextWriter original      = Console.Out;
    _                        = instance.Register(original);
    Console.SetOut(instance);
    return instance;
  });

  /// <inheritdoc cref="MultiTextWriter.Register(TextWriter)"/>
  public static Guid Register(TextWriter w) {
    return singleton_.Value.Register(w);
  }

  /// <inheritdoc cref="MultiTextWriter.Unregister(Guid)"/>
  public static bool Unregister(Guid guid) {
    return singleton_.Value.Unregister(guid);
  }
}

/**
 * <summary>
 *  A <see cref="TextWriter"/> that delegates write calls to multiple other writers.
 * </summary>
 */
public class MultiTextWriter : TextWriter {
  public override Encoding Encoding                                  => Encoding.UTF8;
  protected readonly ConcurrentDictionary<Guid, TextWriter> writers_  = new();

  /**
   * <summary>
   *  Registers a <see cref="TextWriter"/> to also be written to with this.
   * </summary>
   * <returns>
   *  A <see cref="Guid"/> to use with <see cref="Unregister(Guid)"/>.
   * </returns>
   * <exception cref="ArgumentNullException">
   *  Thrown when <paramref name="w"/> is <see langword="null"/>
   * </exception>
   */
  public Guid Register(TextWriter w) {
#pragma warning disable CA1510  // Use ArgumentNullException throw helper
    if (w is null)
      throw new ArgumentNullException(nameof(w));
#pragma warning restore CA1510  // Use ArgumentNullException throw helper
    Guid guid = Guid.NewGuid();
    writers_.TryAdd(guid, w);
    return guid;
  }

  /**
   * <summary>
   *  Removes a <see cref="TextWriter"/> from being written by this.
   * </summary>
   * <param name="guid">
   *  The <see cref="Guid"/> you got from <see cref="Register(TextWriter)"/>
   * </param>
   * <returns>
   *  <c>true</c> if the <see cref="TextWriter"/> was removed successfully; otherwise,
   * <c>false</c>.
   * </returns>
   */
  public bool Unregister(Guid guid) {
    return writers_.TryRemove(guid, out _);
  }

  private async Task WriteToAllAsync(Func<TextWriter, Task> write_ops) {
    Dictionary<Guid, TextWriter> writers = new(writers_);
    List<Task> tasks                     = new(writers.Count);

    foreach ((Guid guid, TextWriter writer) in writers) {
      tasks.Add(WriteOneAsync(guid, writer, write_ops));
    }

    await Task.WhenAll(tasks).ConfigureAwait(false);
  }

  private async Task WriteOneAsync(Guid guid, TextWriter writer, Func<TextWriter, Task> write_ops) {
    try {
      await write_ops(writer).ConfigureAwait(false);
    } catch (ObjectDisposedException) {
      Unregister(guid);
    }
  }

  public override Task WriteAsync(char value) {
    return WriteToAllAsync(w => w.WriteAsync(value));
  }

  public override Task WriteAsync(string? value) {
    return value == null ? Task.CompletedTask : WriteToAllAsync(w => w.WriteAsync(value));
  }

  public override Task WriteAsync(char[] buffer, int index, int count) {
    return WriteToAllAsync(w => w.WriteAsync(buffer, index, count));
  }

  public override Task WriteAsync(ReadOnlyMemory<char> buffer,
      CancellationToken cancellationToken = default) {
    return WriteToAllAsync(w => w.WriteAsync(buffer, cancellationToken));
  }

  private void WriteToAll(Action<TextWriter> write_ops) {
    Dictionary<Guid, TextWriter> writers = new(writers_);
    foreach ((Guid guid, TextWriter writer) in writers) {
      WriteOne(guid, writer, write_ops);
    }
  }

  private void WriteOne(Guid guid, TextWriter writer, Action<TextWriter> write_ops) {
    try {
      write_ops(writer);
    } catch (ObjectDisposedException) {
      Unregister(guid);
    }
  }

  public override void Write(char value) => WriteToAll(w => w.Write(value));

  public override void Write(string? value) {
    if (value == null)
      return;
    WriteToAll(w => w.Write(value));
  }

  public override void Write(char[] buffer, int index, int count) {
    WriteToAll(w => w.Write(buffer, index, count));
  }

  public override void Flush() {
    Dictionary<Guid, TextWriter> writers = new(writers_);
    foreach ((_, TextWriter writer) in writers) {
      writer.Flush();
    }
  }

  public override async Task FlushAsync() {
    Dictionary<Guid, TextWriter> writers = new(writers_);
    List<Task> tasks                     = new(writers.Count);

    foreach (var (_, writer) in writers) {
      tasks.Add(writer.FlushAsync());
    }
    await Task.WhenAll(tasks).ConfigureAwait(false);
  }

  protected override void Dispose(bool disposing) {
    if (disposing) {
      Dictionary<Guid, TextWriter> writers = new(writers_);
      foreach ((_, TextWriter writer) in writers) {
        writer.Dispose();
      }
    }
    base.Dispose(disposing);
  }
}
}
