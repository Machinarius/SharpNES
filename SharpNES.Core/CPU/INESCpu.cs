﻿using SharpNES.Core.DataBus;

namespace SharpNES.Core.CPU {
  public interface INESCpu {
    NESCpuFlags StatusRegister { get; }
    byte AccumulatorRegister { get; }
    byte XRegister { get; }
    byte YRegister { get; }
    byte StackPointer { get; }
    ushort ProgramCounter { get; }
    byte ALUInputRegister { get; set; }

    ICpuInstructionExecutor InstructionExecutor { get; }
    IMemoryAddressingModes AddressingModes { get; }

    void ConnectToDataBus(INESDataBus dataBus);

    void OnClockTick();

    void OnResetRequested();

    void OnInterruptRequested();

    void OnNonMaskableInterruptRequested();
  }
}
