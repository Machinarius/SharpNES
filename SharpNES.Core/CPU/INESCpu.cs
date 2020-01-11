﻿using SharpNES.Core.DataBus;

namespace SharpNES.Core.CPU {
  public interface INESCpu {
    NESCpuFlags StatusRegister { get; }
    byte AccumulatorRegister { get; }
    byte XRegister { get; }
    byte YRegister { get; }
    byte StackPointer { get; }
    ushort ProgramCounter { get; set; }
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
  }
}
