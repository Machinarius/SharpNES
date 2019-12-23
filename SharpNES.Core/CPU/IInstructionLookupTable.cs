namespace SharpNES.Core.CPU {
  public interface IInstructionLookupTable {
    CpuInstruction GetInstructionForOpCode(byte opCode);
  }
}
