using SharpNES.Core.DataBus;

namespace SharpNES.Core.CPU {
  public interface INESCpu {
    NESCpuFlags StatusRegister { get; set; }
    byte AccumulatorRegister { get; set; }
    byte XRegister { get; }
    byte YRegister { get; }
    byte StackPointer { get; }
    ushort ProgramCounter { get; set; }
    /// <summary>
    /// This is a Pseudo-Register that does not actually
    /// exist in the MOS6502 CPU. We have this here to make
    /// it easier for some Op-Codes to get the operand from
    /// the memory address supplied by the addressing mode
    /// </summary>
    byte ALUInputRegister { get; set; }
    ushort AbsoluteAddress { get; set; }
    ushort RelativeAddress { get; set; }

    ICpuInstructionExecutor InstructionExecutor { get; }
    IMemoryAddressingModes AddressingModes { get; }

    void ConnectToDataBus(INESDataBus dataBus);

    void OnClockTick();

    void Reset();

    void OnInterruptRequested();

    void OnNonMaskableInterruptRequested();

    byte ReadFromDataBus(ushort address);
    void WriteToDataBus(ushort address, byte dataToWrite);

    void ReadALUInputRegister();
  }
}
