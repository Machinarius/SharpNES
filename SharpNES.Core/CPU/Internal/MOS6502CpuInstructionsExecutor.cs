﻿using System;
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
      var input = _cpu.ReadALUInputRegister();
      var comparison = _cpu.AccumulatorRegister & input;
      
      var isZero = Convert.ToByte(comparison & Masks.LowerBits) == 0;
      if (isZero) {
        _cpu.StatusRegister |= NESCpuFlags.Zero;
      }

      var isNegative = (comparison & Masks.SignBit) > 0;
      if (isNegative) {
        _cpu.StatusRegister |= NESCpuFlags.Negative;
      }

      var triggersOverflow = (comparison & Masks.OverflowBit) > 0;
      if (triggersOverflow) {
        _cpu.StatusRegister |= NESCpuFlags.Overflow;
      }

      return 0;
    }

    // This is the heart of all branch instructions. They all perform the same
    // PC calculations after they check their respective conditions.
    private int ExecuteBranch() {
      var extraCycles = 1;
      _cpu.AbsoluteAddress = Convert.ToUInt16((_cpu.ProgramCounter + _cpu.RelativeAddress) & Masks.TwoBytes);

      var pageJumpOcurred = (_cpu.AbsoluteAddress & Masks.HigherBits) != (_cpu.ProgramCounter & Masks.HigherBits);
      if (pageJumpOcurred) {
        extraCycles += 1;
      }

      _cpu.ProgramCounter = _cpu.AbsoluteAddress;
      return extraCycles;
    }

    public int BranchOnCarryClear() {
      if (_cpu.StatusRegister.HasFlag(NESCpuFlags.CarryBit)) {
        return 0;
      }
      
      return ExecuteBranch();
    }

    public int BranchOnCarrySet() {
      if (!_cpu.StatusRegister.HasFlag(NESCpuFlags.CarryBit)) {
        return 0;
      }
      
      return ExecuteBranch();
    }

    public int BranchOnEqual() {
      if (!_cpu.StatusRegister.HasFlag(NESCpuFlags.Zero)) {
        return 0;
      }
      
      return ExecuteBranch();
    }

    public int BranchOnMinus() {
      if (!_cpu.StatusRegister.HasFlag(NESCpuFlags.Negative)) {
        return 0;
      }
      
      return ExecuteBranch();
    }

    public int BranchOnNotEqual() {
      if (_cpu.StatusRegister.HasFlag(NESCpuFlags.Zero)) {
        return 0;
      }
      
      return ExecuteBranch();
    }

    public int BranchOnOverflowClear() {
      if (_cpu.StatusRegister.HasFlag(NESCpuFlags.Overflow)) {
        return 0;
      }

      return ExecuteBranch();
    }

    public int BranchOnOverflowSet() {
      if (!_cpu.StatusRegister.HasFlag(NESCpuFlags.Overflow)) {
        return 0;
      }

      return ExecuteBranch();
    }

    public int BranchOnPlus() {
      if (_cpu.StatusRegister.HasFlag(NESCpuFlags.Negative)) {
        return 0;
      }

      return ExecuteBranch();
    }

    public int BreakInterrupt() {
      _cpu.ProgramCounter++;
      _cpu.StatusRegister |= NESCpuFlags.DisableInterrupts;

      var higherPcBits = Convert.ToByte((_cpu.ProgramCounter >> 8) & Masks.LowerBits);
      var lowerPcBits = Convert.ToByte(_cpu.ProgramCounter & Masks.LowerBits);
      ushort getActualStackLocation() => Convert.ToUInt16(Constants.StackBase + _cpu.StackPointer);

      _cpu.WriteToDataBus(getActualStackLocation(), higherPcBits);
      _cpu.StackPointer--;

      _cpu.WriteToDataBus(getActualStackLocation(), lowerPcBits);
      _cpu.StackPointer--;

      _cpu.StatusRegister |= NESCpuFlags.Break;
      _cpu.WriteToDataBus(getActualStackLocation(), (byte)_cpu.StatusRegister);
      _cpu.StackPointer--;

      _cpu.StatusRegister &= ~NESCpuFlags.Break;

      lowerPcBits = _cpu.ReadFromDataBus(Constants.InterruptHandlerLowAddress);
      higherPcBits = _cpu.ReadFromDataBus(Constants.InterruptHandlerHighAddress);
      _cpu.ProgramCounter = Convert.ToUInt16(higherPcBits << 8 | lowerPcBits);

      return 0;
    }

    public int ClearCarry() {
      _cpu.StatusRegister &= ~NESCpuFlags.CarryBit;
      return 0;
    }

    public int ClearDecimal() {
      _cpu.StatusRegister &= ~NESCpuFlags.DecimalMode;
      return 0;
    }

    public int ClearInterruptDisable() {
      throw new NotImplementedException();
    }

    public int ClearOverflow() {
      _cpu.StatusRegister &= ~NESCpuFlags.Overflow;
      return 0;
    }

    public int CompareWithAccumulator() {
      var aluInput = _cpu.ReadALUInputRegister();
      var result = _cpu.AccumulatorRegister - aluInput;
      if (_cpu.AccumulatorRegister >= aluInput) _cpu.StatusRegister |= NESCpuFlags.CarryBit;
      if ((result & Masks.LowerBits) == 0) _cpu.StatusRegister |= NESCpuFlags.Zero;
      if ((result & Masks.SignBit) == Masks.SignBit) _cpu.StatusRegister |= NESCpuFlags.Negative;

      return 1;
    }

    public int CompareWithX() {
      var aluInput = _cpu.ReadALUInputRegister();
      var result = _cpu.XRegister - aluInput;
      if (_cpu.XRegister >= aluInput) _cpu.StatusRegister |= NESCpuFlags.CarryBit;
      if ((result & Masks.LowerBits) == 0) _cpu.StatusRegister |= NESCpuFlags.Zero;
      if ((result & Masks.SignBit) == Masks.SignBit) _cpu.StatusRegister |= NESCpuFlags.Negative;

      return 1;
    }

    public int CompareWithY() {
      var aluInput = _cpu.ReadALUInputRegister();
      var result = _cpu.YRegister - aluInput;
      if (_cpu.YRegister >= aluInput) _cpu.StatusRegister |= NESCpuFlags.CarryBit;
      if ((result & Masks.LowerBits) == 0) _cpu.StatusRegister |= NESCpuFlags.Zero;
      if ((result & Masks.SignBit) == Masks.SignBit) _cpu.StatusRegister |= NESCpuFlags.Negative;

      return 1;
    }

    public int Decrement() {
      var aluInput = _cpu.ReadALUInputRegister();
      var result = aluInput - 1;
      _cpu.WriteToDataBus(_cpu.AbsoluteAddress, Convert.ToByte(result & Masks.LowerBits));
      
      if ((result & Masks.LowerBits) == 0) {
        _cpu.StatusRegister |= NESCpuFlags.Zero;
      }

      if ((result & Masks.SignBit) == Masks.SignBit) {
        _cpu.StatusRegister |= NESCpuFlags.Negative;
      }

      return 0;
    }

    public int DecrementX() {
      _cpu.XRegister--;
      if (_cpu.XRegister == 0) {
        _cpu.StatusRegister |= NESCpuFlags.Zero;
      }

      if ((_cpu.XRegister & Masks.SignBit) == Masks.SignBit) {
        _cpu.StatusRegister |= NESCpuFlags.Negative;
      }

      return 0;
    }

    public int DecrementY() {
      _cpu.YRegister--;
      if (_cpu.YRegister == 0) {
        _cpu.StatusRegister |= NESCpuFlags.Zero;
      }

      if ((_cpu.YRegister & Masks.SignBit) == Masks.SignBit) {
        _cpu.StatusRegister |= NESCpuFlags.Negative;
      }

      return 0;
    }

    public int ExclusiveOr() {
      var aluInput = _cpu.ReadALUInputRegister();
      _cpu.AccumulatorRegister ^= aluInput;
      
      if (_cpu.AccumulatorRegister == 0) {
        _cpu.StatusRegister |= NESCpuFlags.Zero;
      }

      if ((_cpu.AccumulatorRegister & Masks.SignBit) == Masks.SignBit) {
        _cpu.StatusRegister |= NESCpuFlags.Negative;
      }

      return 1;
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
      public const ushort SignBit = 1 << 7;
      public const ushort OverflowBit = 1 << 6;
    }

    private static class Constants {
      public const ushort StackBase = 0x0100;
      public const ushort InterruptHandlerLowAddress = 0xFFFE;
      public const ushort InterruptHandlerHighAddress = 0xFFFF;
    }
  }
}
