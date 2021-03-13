using SharpNES.Core.DataBus;

namespace SharpNES.Core.CPU {
  public interface INESCpu {
    NESCpuFlags StatusRegister { get; set; }
    byte AccumulatorRegister { get; set; }
    byte XRegister { get; set; }
    byte YRegister { get; set; }
    byte StackPointer { get; set; }
    ushort ProgramCounter { get; set; }
    ushort AbsoluteAddress { get; set; }
    ushort RelativeAddress { get; set; }

    CpuInstruction CurrentInstruction { get; }

    ICpuInstructionExecutor InstructionExecutor { get; }
    IMemoryAddressingModes AddressingModes { get; }

    void ConnectToDataBus(INESDataBus dataBus);

    void OnClockTick();

    void Reset();

    void OnInterruptRequested();

    void OnNonMaskableInterruptRequested();

    byte ReadFromDataBus(ushort address);
    void WriteToDataBus(ushort address, byte dataToWrite);

    byte ReadALUInputRegister();
  }
}
