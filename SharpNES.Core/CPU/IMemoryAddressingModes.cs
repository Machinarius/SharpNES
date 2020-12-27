namespace SharpNES.Core.CPU {
  // All of these functions return true in scenarios
  // they need an additional theoretical cycle
  public interface IMemoryAddressingModes {
    // IMP
    int Implicit();
    // IMM
    int Immediate();
    // ZP0
    int ZeroPageZero();
    // ZPX
    int ZeroPageX();
    // ZPY
    int ZeroPageY();
    // REL
    int Relative();
    // ABS
    int Absolute();
    // ABX
    int AbsoluteX();
    // ABY
    int AbsoluteY();
    // IND
    int Indirect();
    // IZX
    int IndirectX();
    // IZY
    int IndirectY();
  }
}
