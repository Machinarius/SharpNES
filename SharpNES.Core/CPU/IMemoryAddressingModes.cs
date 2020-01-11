namespace SharpNES.Core.CPU {
  // All of these functions return true in scenarios
  // they need an additional theoretical cycle
  public interface IMemoryAddressingModes {
    // IMP
    bool Implicit();
    // IMM
    bool Immediate();
    // ZP0
    bool ZeroPageZero();
    // ZPX
    bool ZeroPageX();
    // ZPY
    bool ZeroPageY();
    // REL
    bool Relative();
    // ABS
    bool Absolute();
    // ABX
    bool AbsoluteX();
    // ABY
    bool AbsoluteY();
    // IND
    bool Indirect();
    // IZX
    bool IndirectX();
    // IZY
    bool IndirectY();
  }
}
