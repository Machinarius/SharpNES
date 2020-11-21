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

    public bool AddWithCarry() {
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

      return true;
    }

    public bool AndWithAccumulator() {
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

      return true;
    }

    public bool ArithmeticShiftLeft() {
      throw new NotImplementedException();
    }

    public bool BitTest() {
      throw new NotImplementedException();
    }

    public bool BranchOnCarryClear() {
      throw new NotImplementedException();
    }

    public bool BranchOnCarrySet() {
      throw new NotImplementedException();
    }

    public bool BranchOnEqual() {
      throw new NotImplementedException();
    }

    public bool BranchOnMinus() {
      throw new NotImplementedException();
    }

    public bool BranchOnNotEqual() {
      throw new NotImplementedException();
    }

    public bool BranchOnOverflowClear() {
      throw new NotImplementedException();
    }

    public bool BranchOnOverflowSet() {
      throw new NotImplementedException();
    }

    public bool BranchOnPlus() {
      throw new NotImplementedException();
    }

    public bool BreakInterrupt() {
      throw new NotImplementedException();
    }

    public bool ClearCarry() {
      throw new NotImplementedException();
    }

    public bool ClearDecimal() {
      throw new NotImplementedException();
    }

    public bool ClearInterruptDisable() {
      throw new NotImplementedException();
    }

    public bool ClearOverflow() {
      throw new NotImplementedException();
    }

    public bool CompareWithAccumulator() {
      throw new NotImplementedException();
    }

    public bool CompareWithX() {
      throw new NotImplementedException();
    }

    public bool CompareWithY() {
      throw new NotImplementedException();
    }

    public bool Decrement() {
      throw new NotImplementedException();
    }

    public bool DecrementX() {
      throw new NotImplementedException();
    }

    public bool DecrementY() {
      throw new NotImplementedException();
    }

    public bool ExclusiveOr() {
      throw new NotImplementedException();
    }

    public bool IllegalOpCode() {
      throw new NotImplementedException();
    }

    public bool Increment() {
      throw new NotImplementedException();
    }

    public bool IncrementX() {
      throw new NotImplementedException();
    }

    public bool IncrementY() {
      throw new NotImplementedException();
    }

    public bool Jump() {
      throw new NotImplementedException();
    }

    public bool JumpSubRoutine() {
      throw new NotImplementedException();
    }

    public bool LoadAccumulator() {
      throw new NotImplementedException();
    }

    public bool LoadX() {
      throw new NotImplementedException();
    }

    public bool LoadY() {
      throw new NotImplementedException();
    }

    public bool LogicalShiftRight() {
      throw new NotImplementedException();
    }

    public bool NoOperation() {
      throw new NotImplementedException();
    }

    public bool OrWithAccumulator() {
      throw new NotImplementedException();
    }

    public bool PullAccumulator() {
      throw new NotImplementedException();
    }

    public bool PullProcessorStatus() {
      throw new NotImplementedException();
    }

    public bool PushAccumulator() {
      throw new NotImplementedException();
    }

    public bool PushProcessorStatus() {
      throw new NotImplementedException();
    }

    public bool ReturnFromInterrupt() {
      throw new NotImplementedException();
    }

    public bool ReturnFromSubroutine() {
      throw new NotImplementedException();
    }

    public bool RotateLeft() {
      throw new NotImplementedException();
    }

    public bool RotateRight() {
      throw new NotImplementedException();
    }

    public bool SetCarry() {
      throw new NotImplementedException();
    }

    public bool SetDecimal() {
      throw new NotImplementedException();
    }

    public bool SetInterruptDisable() {
      throw new NotImplementedException();
    }

    public bool StoreAccumulator() {
      throw new NotImplementedException();
    }

    public bool StoreX() {
      throw new NotImplementedException();
    }

    public bool StoreY() {
      throw new NotImplementedException();
    }

    public bool SubtractWithCarry() {
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

      return true;
    }

    public bool TransferAccumulatorToX() {
      throw new NotImplementedException();
    }

    public bool TransferAccumulatorToY() {
      throw new NotImplementedException();
    }

    public bool TransferStackPointerToX() {
      throw new NotImplementedException();
    }

    public bool TransferXToAccumulator() {
      throw new NotImplementedException();
    }

    public bool TransferXToStackPointer() {
      throw new NotImplementedException();
    }

    public bool TransferYToAccumulator() {
      throw new NotImplementedException();
    }

    private static class Masks {
      public const int HigherBits = 0xFF00;
      public const int LowerBits = 0x00FF;
      public const int SignBit = 0x80;
    }
  }
}
