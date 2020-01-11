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

    public NESCpuFlags StatusRegister { get; private set; }

    public byte AccumulatorRegister => throw new NotImplementedException();

    public byte XRegister => throw new NotImplementedException();

    public byte YRegister => throw new NotImplementedException();

    public byte ALUInputRegister { get; set; }

    public byte StackPointer => throw new NotImplementedException();

    public ushort ProgramCounter { get; set; }

    public ushort AbsoluteAddress { get; set; }
    
    public ushort RelativeAddress { get; set; }

    public ICpuInstructionExecutor InstructionExecutor => throw new NotImplementedException();

    public IMemoryAddressingModes AddressingModes => throw new NotImplementedException();

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

    public void OnResetRequested() {
      throw new NotImplementedException();
    }

    public void OnInterruptRequested() {
      throw new NotImplementedException();
    }

    public void OnNonMaskableInterruptRequested() {
      throw new NotImplementedException();
    }

    private void WriteToMemory(ushort address, byte dataToWrite) {
      _dataBus.WriteToMemory(address, dataToWrite);
    }

    public byte ReadFromDataBus(ushort address) {
      return _dataBus.ReadFromMemory(address, false);
    }

    private void SetStatusFlagValue(NESCpuFlags flag, bool value) {
      if (value) {
        StatusRegister |= flag;
      } else {
        StatusRegister &= ~flag;
      }
    }

    private bool GetStatusFlagValue(NESCpuFlags flag) {
      return (StatusRegister & flag) == flag;
    }
  }
}
