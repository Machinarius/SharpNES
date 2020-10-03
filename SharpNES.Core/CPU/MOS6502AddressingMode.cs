namespace SharpNES.Core.CPU {
  public enum MOS6502AddressingMode {
    Invalid,
    Implicit,
    Immediate,
    ZeroPageZero,
    ZeroPageX,
    ZeroPageY,
    Relative,
    Absolute,
    AbsoluteX,
    AbsoluteY,
    Indirect,
    IndirectX,
    IndirectY
  }
}
