using System;
using Microsoft.Extensions.Logging;

namespace SharpNES.Core.CPU.Internal {
  public class MOS6502CpuInstructionExecutor : ICpuInstructionExecutor {
    private readonly INESCpu _cpu;
    private readonly ILogger<MOS6502CpuInstructionExecutor> _logger;

    public MOS6502CpuInstructionExecutor(INESCpu cpu, ILogger<MOS6502CpuInstructionExecutor> logger) {
      _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int AddWithCarry() {
      var aluInput = _cpu.ReadALUInputRegister();

      var addResult = aluInput + _cpu.AccumulatorRegister +
         (_cpu.StatusRegister.HasFlag(NESCpuFlags.CarryBit) ? 1 : 0);
      var carryBit = addResult > byte.MaxValue;
      var zeroBit = (addResult & Masks.LowerBits) == 0;
      var negativeBit = Convert.ToBoolean(addResult & Masks.SignBit);
      var overflowBit = Convert.ToBoolean(
        (~(_cpu.AccumulatorRegister ^ aluInput) & (_cpu.AccumulatorRegister ^ addResult)) & Masks.SignBit
      );

      var resultFlags = _cpu.StatusRegister;
      if (carryBit) {
        resultFlags |= NESCpuFlags.CarryBit;
      }

      if (zeroBit) {
        resultFlags |= NESCpuFlags.Zero;
      }

      if (negativeBit) {
        resultFlags |= NESCpuFlags.Negative;
      }

      if (overflowBit) {
        resultFlags |= NESCpuFlags.Overflow;
      }

      _cpu.StatusRegister = resultFlags;
      _cpu.AccumulatorRegister = Convert.ToByte(addResult & Masks.LowerBits);

      return 1;
    }

    public int AndWithAccumulator() {
      var input = _cpu.ReadALUInputRegister();
      var accumulator = _cpu.AccumulatorRegister;
      _cpu.AccumulatorRegister = (byte)(input & accumulator);

      var negativeValue = (_cpu.AccumulatorRegister & Masks.SignBit) == Masks.SignBit;
      var zeroValue = _cpu.AccumulatorRegister == 0;

      if (negativeValue) {
        _cpu.StatusRegister |= NESCpuFlags.Negative;
      }

      if (zeroValue) {
        _cpu.StatusRegister |= NESCpuFlags.Zero;
      }

      return 1;
    }

    public int ArithmeticShiftLeft() {
      var aluInput = _cpu.ReadALUInputRegister();
      var result = aluInput << 1;

      if ((result & Masks.HigherBits) > 0) {
        _cpu.StatusRegister |= NESCpuFlags.CarryBit;
      }

      if ((result & Masks.SignBit) == Masks.SignBit) {
        _cpu.StatusRegister |= NESCpuFlags.Negative;
      }

      if ((result & Masks.LowerBits) == 0) {
        _cpu.StatusRegister |= NESCpuFlags.Zero;
      }

      var clampedResult = Convert.ToByte(result & Masks.LowerBits);
      if (_cpu.CurrentInstruction.AddressingMode == MOS6502AddressingMode.Implicit) {
        _cpu.AccumulatorRegister = clampedResult;
      } else {
        _cpu.WriteToDataBus(_cpu.AbsoluteAddress, clampedResult);
      }
      return 0;
    }

    public int BitTest() {
      throw new NotImplementedException();
    }

    public int BranchOnCarryClear() {
      if (_cpu.StatusRegister.HasFlag(NESCpuFlags.CarryBit)) {
        return 0;
      }

      var extraCycles = 1;
      _cpu.AbsoluteAddress = Convert.ToUInt16((_cpu.ProgramCounter + _cpu.RelativeAddress) & Masks.TwoBytes);

      var pageJumpOcurred = (_cpu.AbsoluteAddress & Masks.HigherBits) != (_cpu.ProgramCounter & Masks.HigherBits);
      if (pageJumpOcurred) {
        extraCycles += 1;
      }

      _cpu.ProgramCounter = _cpu.AbsoluteAddress;
      return extraCycles;
    }

    public int BranchOnCarrySet() {
      if (!_cpu.StatusRegister.HasFlag(NESCpuFlags.CarryBit)) {
        return 0;
      }

      var extraCycles = 1;
      _cpu.AbsoluteAddress = Convert.ToUInt16((_cpu.ProgramCounter + _cpu.RelativeAddress) & Masks.TwoBytes);

      var pageJumpOcurred = (_cpu.AbsoluteAddress & Masks.HigherBits) != (_cpu.ProgramCounter & Masks.HigherBits);
      if (pageJumpOcurred) {
        extraCycles += 1;
      }

      _cpu.ProgramCounter = _cpu.AbsoluteAddress;
      return extraCycles;
    }

    public int BranchOnEqual() {
      throw new NotImplementedException();
    }

    public int BranchOnMinus() {
      throw new NotImplementedException();
    }

    public int BranchOnNotEqual() {
      throw new NotImplementedException();
    }

    public int BranchOnOverflowClear() {
      throw new NotImplementedException();
    }

    public int BranchOnOverflowSet() {
      throw new NotImplementedException();
    }

    public int BranchOnPlus() {
      throw new NotImplementedException();
    }

    public int BreakInterrupt() {
      throw new NotImplementedException();
    }

    public int ClearCarry() {
      throw new NotImplementedException();
    }

    public int ClearDecimal() {
      throw new NotImplementedException();
    }

    public int ClearInterruptDisable() {
      throw new NotImplementedException();
    }

    public int ClearOverflow() {
      throw new NotImplementedException();
    }

    public int CompareWithAccumulator() {
      throw new NotImplementedException();
    }

    public int CompareWithX() {
      throw new NotImplementedException();
    }

    public int CompareWithY() {
      throw new NotImplementedException();
    }

    public int Decrement() {
      throw new NotImplementedException();
    }

    public int DecrementX() {
      throw new NotImplementedException();
    }

    public int DecrementY() {
      throw new NotImplementedException();
    }

    public int ExclusiveOr() {
      throw new NotImplementedException();
    }

    public int IllegalOpCode() {
      throw new NotImplementedException();
    }

    public int Increment() {
      throw new NotImplementedException();
    }

    public int IncrementX() {
      throw new NotImplementedException();
    }

    public int IncrementY() {
      throw new NotImplementedException();
    }

    public int Jump() {
      throw new NotImplementedException();
    }

    public int JumpSubRoutine() {
      throw new NotImplementedException();
    }

    public int LoadAccumulator() {
      throw new NotImplementedException();
    }

    public int LoadX() {
      throw new NotImplementedException();
    }

    public int LoadY() {
      throw new NotImplementedException();
    }

    public int LogicalShiftRight() {
      throw new NotImplementedException();
    }

    public int NoOperation() {
      throw new NotImplementedException();
    }

    public int OrWithAccumulator() {
      throw new NotImplementedException();
    }

    public int PullAccumulator() {
      throw new NotImplementedException();
    }

    public int PullProcessorStatus() {
      throw new NotImplementedException();
    }

    public int PushAccumulator() {
      throw new NotImplementedException();
    }

    public int PushProcessorStatus() {
      throw new NotImplementedException();
    }

    public int ReturnFromInterrupt() {
      throw new NotImplementedException();
    }

    public int ReturnFromSubroutine() {
      throw new NotImplementedException();
    }

    public int RotateLeft() {
      throw new NotImplementedException();
    }

    public int RotateRight() {
      throw new NotImplementedException();
    }

    public int SetCarry() {
      throw new NotImplementedException();
    }

    public int SetDecimal() {
      throw new NotImplementedException();
    }

    public int SetInterruptDisable() {
      throw new NotImplementedException();
    }

    public int StoreAccumulator() {
      throw new NotImplementedException();
    }

    public int StoreX() {
      throw new NotImplementedException();
    }

    public int StoreY() {
      throw new NotImplementedException();
    }

    public int SubtractWithCarry() {
      var aluInput = _cpu.ReadALUInputRegister();

      var twosComplement = (aluInput ^ Masks.LowerBits) + 1;
      var subtractionResult = _cpu.AccumulatorRegister + twosComplement +
        (_cpu.StatusRegister.HasFlag(NESCpuFlags.CarryBit) ? 1 : 0);
      var carryBit = Convert.ToBoolean((subtractionResult & 0xFFFF) & Masks.HigherBits);
      var zeroBit = (subtractionResult & Masks.LowerBits) == 0;
      var negativeBit = Convert.ToBoolean(subtractionResult & Masks.SignBit);
      var overflowBit = Convert.ToBoolean(
        (subtractionResult ^ _cpu.AccumulatorRegister) & (subtractionResult ^ twosComplement) & Masks.SignBit
      );

      var resultFlags = _cpu.StatusRegister;
      if (carryBit) {
        resultFlags |= NESCpuFlags.CarryBit;
      }

      if (zeroBit) {
        resultFlags |= NESCpuFlags.Zero;
      }

      if (negativeBit) {
        resultFlags |= NESCpuFlags.Negative;
      }

      if (overflowBit) {
        resultFlags |= NESCpuFlags.Overflow;
      }

      _cpu.StatusRegister = resultFlags;
      _cpu.AccumulatorRegister = Convert.ToByte(subtractionResult & Masks.LowerBits);

      return 1;
    }

    public int TransferAccumulatorToX() {
      throw new NotImplementedException();
    }

    public int TransferAccumulatorToY() {
      throw new NotImplementedException();
    }

    public int TransferStackPointerToX() {
      throw new NotImplementedException();
    }

    public int TransferXToAccumulator() {
      throw new NotImplementedException();
    }

    public int TransferXToStackPointer() {
      throw new NotImplementedException();
    }

    public int TransferYToAccumulator() {
      throw new NotImplementedException();
    }

    private static class Masks {
      public const ushort TwoBytes = 0xFFFF;
      public const ushort HigherBits = 0xFF00;
      public const ushort LowerBits = 0x00FF;
      public const ushort SignBit = 0x80;
    }
  }
}
