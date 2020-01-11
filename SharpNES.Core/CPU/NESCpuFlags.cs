using System;

namespace SharpNES.Core.CPU {
  [Flags]
  public enum NESCpuFlags : byte {
    Null = 0,
    CarryBit = 1 << 0,
    Zero = 1 << 1,
    DisableInterrupts = 1 << 2,
    DecimalMode = 1 << 3,
    Break = 1 << 4,
    Unused = 1 << 5,
    Overflow = 1 << 6,
    Negative = 1 << 7
  }
}
