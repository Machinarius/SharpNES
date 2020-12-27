using System;

namespace SharpNES.Core.CPU {
  public struct CpuInstruction : IEquatable<CpuInstruction> {
    public MOS6502Instruction Instruction { get; set; }
    public MOS6502AddressingMode AddressingMode { get; set; }
    public Func<int> OperatorFunc { get; set; }
    public Func<int> AddressingModeFunc { get; set; }
    public byte BaseCycleCount { get; set; }

    public CpuInstruction(MOS6502Instruction code, MOS6502AddressingMode addrMode,
      Func<int> operatorFunc, Func<int> addressingModeFunc, byte baseCycleCount) {
      Instruction = code;
      AddressingMode = addrMode;
      OperatorFunc = operatorFunc ?? throw new ArgumentNullException(nameof(operatorFunc));
      AddressingModeFunc = addressingModeFunc ?? throw new ArgumentNullException(nameof(addressingModeFunc));
      BaseCycleCount = baseCycleCount;
    }

    public override bool Equals(object obj) {
      if (obj is CpuInstruction other) {
        return this.Equals(other);
      }

      return false;
    }

    public override int GetHashCode() {
      return HashCode.Combine(Instruction, AddressingMode);
    }

    public static bool operator ==(CpuInstruction left, CpuInstruction right) {
      return left.Equals(right);
    }

    public static bool operator !=(CpuInstruction left, CpuInstruction right) {
      return !(left == right);
    }

    public bool Equals(CpuInstruction other) {
      return 
        Instruction == other.Instruction && 
        AddressingMode == other.AddressingMode;
    }
  }
}
