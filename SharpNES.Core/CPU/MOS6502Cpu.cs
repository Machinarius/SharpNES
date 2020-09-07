using Microsoft.Extensions.Logging;
using SharpNES.Core.CPU.Exceptions;
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

    public int ClockCyclesRemaining { get; private set; }

    // The last memory address used
    private ushort _absoluteAddress;
    // The last address JMP'd to
    private ushort _relativeAddress;

    private CpuInstruction _currentInstruction;

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

      if (ClockCyclesRemaining == 0) { 
        _logger.LogInformation("Fetching next instruction");

        var currentOpCode = ReadFromDataBus(ProgramCounter++);
        _currentInstruction = _instructionsTable.GetInstructionForOpCode(currentOpCode);
        ClockCyclesRemaining = _currentInstruction.CycleCount;

        var addrModeCycle = _currentInstruction.AddressingModeFunc();
        var opCodeCyle = _currentInstruction.OperatorFunc();
        ClockCyclesRemaining += Convert.ToInt32(addrModeCycle && opCodeCyle);
        _logger.LogInformation($"Executing {_currentInstruction.Name} for {ClockCyclesRemaining} cycles");
      }

      ClockCyclesRemaining -= 1;
      _logger.LogDebug($"Consumed cycle. Remaining cycles: {ClockCyclesRemaining}");
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

      ClockCyclesRemaining = 8;
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

      ClockCyclesRemaining = 7;
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

      ClockCyclesRemaining = 8;
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

    public byte ReadALUInputRegister() {
      if (_currentInstruction.AddressingModeIsImplicit) {
        throw new AddressingModeException("Reading the ALU Input Pseudo-Register is not supported in Implicit Mode");
      }

      return ReadFromDataBus(AbsoluteAddress);
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
