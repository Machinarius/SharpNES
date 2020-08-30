using Microsoft.Extensions.Logging;
using SharpNES.Core.DataBus;
using System;

namespace SharpNES.Core.CPU {
  public partial class MOS6502Cpu : INESCpu {
    private INESDataBus _dataBus;
    private readonly ILogger<MOS6502Cpu> _logger;
    private readonly ICpuInstructionExecutor _executor;
    private readonly IMemoryAddressingModes _addressingModes;
    private readonly IInstructionLookupTable _instructionsTable;

    public NESCpuFlags StatusRegister { get; set; }

    public byte AccumulatorRegister { get; set; }

    public byte XRegister { get; set; }

    public byte YRegister { get; set; }

    public byte ALUInputRegister { get; set; }

    public byte StackPointer { get; set; }

    public ushort ProgramCounter { get; set; }

    public ushort AbsoluteAddress { get; set; }
    
    public ushort RelativeAddress { get; set; }

    public ICpuInstructionExecutor InstructionExecutor => throw new NotImplementedException();

    public IMemoryAddressingModes AddressingModes => throw new NotImplementedException();

    public int ClockCyclesRemaining => _remainingCycles;

    // The last memory address used
    private ushort _absoluteAddress;
    // The last address JMP'd to
    private ushort _relativeAddress;
    // Current OpCode being executed
    private byte _currentOpCode;
    // The remaining cycles for the current OpCode
    private int _remainingCycles;

    public MOS6502Cpu(
        ILogger<MOS6502Cpu> logger,
        ICpuInstructionExecutor executor,
        IMemoryAddressingModes addressingModes,
        IInstructionLookupTable instructionsTable) {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _executor = executor ?? throw new ArgumentNullException(nameof(executor));
      _addressingModes = addressingModes ?? throw new ArgumentNullException(nameof(addressingModes));
      _instructionsTable = instructionsTable ?? throw new ArgumentNullException(nameof(instructionsTable));
    }

    public void ConnectToDataBus(INESDataBus dataBus) {
      _dataBus = dataBus ?? throw new ArgumentNullException(nameof(dataBus));
    }

    private byte FetchData() {
      throw new NotImplementedException();
    }

    private CpuInstruction LookupInstruction() {
      throw new NotImplementedException();
    }

    public void OnClockTick() {
      _logger.LogInformation("Beginning clock tick");

      if (_remainingCycles == 0) { 
        _logger.LogInformation("Fetching next instruction");

        _currentOpCode = ReadFromDataBus(ProgramCounter++);
        var instruction = _instructionsTable.GetInstructionForOpCode(_currentOpCode);
        _remainingCycles = instruction.CycleCount;

        var additionalCycles = 0;
        additionalCycles += instruction.AddressingModeFunc() ? 1 : 0;
        additionalCycles += instruction.OperatorFunc() ? 1 : 0;
        _remainingCycles += additionalCycles;

        _logger.LogInformation($"Executing {instruction.Name} for {_remainingCycles} cycles");
      }

      _remainingCycles -= 1;
      _logger.LogDebug($"Consumed cycle. Remaining cycles: {_remainingCycles}");
    }

    public void Reset() {
      AbsoluteAddress = Constants.StartupAddress;
      var pcLowBits = ReadFromDataBus(AbsoluteAddress);
      var pcHighBits = ReadFromDataBus(++AbsoluteAddress);

      ProgramCounter = (ushort)((pcHighBits << 8) | pcLowBits);
      AccumulatorRegister = 0;
      XRegister = 0;
      YRegister = 0;
      StackPointer = Constants.StartupStackPointer;
      StatusRegister = NESCpuFlags.Unused;

      AbsoluteAddress = 0x0;
      RelativeAddress = 0x0;
      ALUInputRegister = 0x0;

      _remainingCycles = 8;
    }

    public void OnInterruptRequested() {
      if (StatusRegister.HasFlag(NESCpuFlags.DisableInterrupts)) {
        return;
      }

      WriteToDataBus(
        (ushort)(Constants.InterruptStackPointerBase + StackPointer--), 
        (byte)((ProgramCounter >> 8) & Constants.Masks.LowByte));
      WriteToDataBus(
        (ushort)(Constants.InterruptStackPointerBase + StackPointer--),
        (byte)(ProgramCounter & Constants.Masks.LowByte));

      SetStatusFlag(NESCpuFlags.Break, false);
      SetStatusFlag(NESCpuFlags.Unused, true);
      SetStatusFlag(NESCpuFlags.DisableInterrupts, true);
      
      WriteToDataBus(
        (ushort)(Constants.InterruptStackPointerBase + StackPointer--),
        (byte)StatusRegister);

      AbsoluteAddress = Constants.InterruptRequestPCAddress;
      var pcLowBits = ReadFromDataBus(AbsoluteAddress);
      var pcHighBits = ReadFromDataBus((ushort)(AbsoluteAddress + 1));
      ProgramCounter = (ushort)((pcHighBits << 8) | pcLowBits);

      _remainingCycles = 7;
    }

    public void OnNonMaskableInterruptRequested() {
      WriteToDataBus(
        (ushort)(Constants.InterruptStackPointerBase + StackPointer--),
        (byte)((ProgramCounter >> 8) & Constants.Masks.LowByte));
      WriteToDataBus(
        (ushort)(Constants.InterruptStackPointerBase + StackPointer--),
        (byte)(ProgramCounter & Constants.Masks.LowByte));

      SetStatusFlag(NESCpuFlags.Break, false);
      SetStatusFlag(NESCpuFlags.Unused, true);
      SetStatusFlag(NESCpuFlags.DisableInterrupts, true);

      WriteToDataBus(
        (ushort)(Constants.InterruptStackPointerBase + StackPointer--),
        (byte)StatusRegister);

      AbsoluteAddress = Constants.NonMaskableInterruptRequestPCAddress;
      var pcLowBits = ReadFromDataBus(AbsoluteAddress);
      var pcHighBits = ReadFromDataBus((ushort)(AbsoluteAddress + 1));
      ProgramCounter = (ushort)((pcHighBits << 8) | pcLowBits);

      _remainingCycles = 8;
    }

    public void WriteToDataBus(ushort address, byte dataToWrite) {
      _dataBus.WriteToMemory(address, dataToWrite);
    }

    public byte ReadFromDataBus(ushort address) {
      return _dataBus.ReadFromMemory(address, false);
    }

    private void SetStatusFlag(NESCpuFlags flag, bool value) {
      if (value) {
        StatusRegister |= flag;
      } else {
        StatusRegister &= ~flag;
      }
    }

    public void ReadALUInputRegister() {
      throw new NotImplementedException();
    }

    private class Constants {
      public const ushort StartupAddress = 0xFFFC;
      public const byte StartupStackPointer = 0xFD;
      public const ushort InterruptStackPointerBase = 0x0100;
      public const ushort InterruptRequestPCAddress = 0xFFFE;
      public const ushort NonMaskableInterruptRequestPCAddress = 0xFFFA;

      public class Masks {
        public const ushort LowByte = 0x00FF;
      }
    }
  }
}
