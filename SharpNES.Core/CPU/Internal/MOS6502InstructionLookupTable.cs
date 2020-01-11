﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace SharpNES.Core.CPU.Internal {
  public class MOS6502InstructionLookupTable : IInstructionLookupTable {
    private readonly ICpuInstructionExecutor _executor;
    private readonly IMemoryAddressingModes _addressingModes;
    private readonly ILogger<MOS6502InstructionLookupTable> _logger;

    private readonly List<CpuInstruction> _instructions;

    public MOS6502InstructionLookupTable(
        ICpuInstructionExecutor executor, 
        IMemoryAddressingModes addressingModes, 
        ILogger<MOS6502InstructionLookupTable> logger) {
      _executor = executor ?? throw new ArgumentNullException(nameof(executor));
      _addressingModes = addressingModes ?? throw new ArgumentNullException(nameof(addressingModes));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));

      _instructions = new List<CpuInstruction>();
      FillInstructionsList();
    }

    public CpuInstruction GetInstructionForOpCode(byte opCode) {
      var opCodeValue = (int)opCode;
      var instruction = _instructions[opCodeValue];
      return instruction;
    }

    private void FillInstructionsList() {
      _instructions.Add(new CpuInstruction("BRK", _executor.BreakInterrupt, _addressingModes.Immediate, 7));
      _instructions.Add(new CpuInstruction("ORA", _executor.OrWithAccumulator, _addressingModes.IndirectX, 6));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 8));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 3));
      _instructions.Add(new CpuInstruction("ORA", _executor.OrWithAccumulator, _addressingModes.ZeroPageZero, 3));
      _instructions.Add(new CpuInstruction("ASL", _executor.ArithmeticShiftLeft, _addressingModes.ZeroPageZero, 5));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 5));
      _instructions.Add(new CpuInstruction("PHP", _executor.PushProcessorStatus, _addressingModes.Implicit, 3));
      _instructions.Add(new CpuInstruction("ORA", _executor.OrWithAccumulator, _addressingModes.Immediate, 2));
      _instructions.Add(new CpuInstruction("ASL", _executor.ArithmeticShiftLeft, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 4));
      _instructions.Add(new CpuInstruction("ORA", _executor.OrWithAccumulator, _addressingModes.Absolute, 4));
      _instructions.Add(new CpuInstruction("ASL", _executor.ArithmeticShiftLeft, _addressingModes.Absolute, 6));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 6));
      _instructions.Add(new CpuInstruction("BPL", _executor.BranchOnPlus, _addressingModes.Relative, 2));
      _instructions.Add(new CpuInstruction("ORA", _executor.OrWithAccumulator, _addressingModes.IndirectY, 5));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 8));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 4));
      _instructions.Add(new CpuInstruction("ORA", _executor.OrWithAccumulator, _addressingModes.ZeroPageX, 4));
      _instructions.Add(new CpuInstruction("ASL", _executor.ArithmeticShiftLeft, _addressingModes.ZeroPageX, 6));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 6));
      _instructions.Add(new CpuInstruction("CLC", _executor.ClearCarry, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("ORA", _executor.OrWithAccumulator, _addressingModes.AbsoluteY, 4));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 7));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 4));
      _instructions.Add(new CpuInstruction("ORA", _executor.OrWithAccumulator, _addressingModes.AbsoluteX, 4));
      _instructions.Add(new CpuInstruction("ASL", _executor.ArithmeticShiftLeft, _addressingModes.AbsoluteX, 7));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 7));
      _instructions.Add(new CpuInstruction("JSR", _executor.JumpSubRoutine, _addressingModes.Absolute, 6));
      _instructions.Add(new CpuInstruction("AND", _executor.AndWithAccumulator, _addressingModes.IndirectX, 6));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 8));
      _instructions.Add(new CpuInstruction("BIT", _executor.BitTest, _addressingModes.ZeroPageZero, 3));
      _instructions.Add(new CpuInstruction("AND", _executor.AndWithAccumulator, _addressingModes.ZeroPageZero, 3));
      _instructions.Add(new CpuInstruction("ROL", _executor.RotateLeft, _addressingModes.ZeroPageZero, 5));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 5));
      _instructions.Add(new CpuInstruction("PLP", _executor.PullProcessorStatus, _addressingModes.Implicit, 4));
      _instructions.Add(new CpuInstruction("AND", _executor.AndWithAccumulator, _addressingModes.Immediate, 2));
      _instructions.Add(new CpuInstruction("ROL", _executor.RotateLeft, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("BIT", _executor.BitTest, _addressingModes.Absolute, 4));
      _instructions.Add(new CpuInstruction("AND", _executor.AndWithAccumulator, _addressingModes.Absolute, 4));
      _instructions.Add(new CpuInstruction("ROL", _executor.RotateLeft, _addressingModes.Absolute, 6));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 6));
      _instructions.Add(new CpuInstruction("BMI", _executor.BranchOnMinus, _addressingModes.Relative, 2));
      _instructions.Add(new CpuInstruction("AND", _executor.AndWithAccumulator, _addressingModes.IndirectY, 5));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 8));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 4));
      _instructions.Add(new CpuInstruction("AND", _executor.AndWithAccumulator, _addressingModes.ZeroPageX, 4));
      _instructions.Add(new CpuInstruction("ROL", _executor.RotateLeft, _addressingModes.ZeroPageX, 6));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 6));
      _instructions.Add(new CpuInstruction("SEC", _executor.SetCarry, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("AND", _executor.AndWithAccumulator, _addressingModes.AbsoluteY, 4));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 7));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 4));
      _instructions.Add(new CpuInstruction("AND", _executor.AndWithAccumulator, _addressingModes.AbsoluteX, 4));
      _instructions.Add(new CpuInstruction("ROL", _executor.RotateLeft, _addressingModes.AbsoluteX, 7));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 7));
      _instructions.Add(new CpuInstruction("RTI", _executor.ReturnFromInterrupt, _addressingModes.Implicit, 6));
      _instructions.Add(new CpuInstruction("EOR", _executor.ExclusiveOr, _addressingModes.IndirectX, 6));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 8));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 3));
      _instructions.Add(new CpuInstruction("EOR", _executor.ExclusiveOr, _addressingModes.ZeroPageZero, 3));
      _instructions.Add(new CpuInstruction("LSR", _executor.LogicalShiftRight, _addressingModes.ZeroPageZero, 5));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 5));
      _instructions.Add(new CpuInstruction("PHA", _executor.PushAccumulator, _addressingModes.Implicit, 3));
      _instructions.Add(new CpuInstruction("EOR", _executor.ExclusiveOr, _addressingModes.Immediate, 2));
      _instructions.Add(new CpuInstruction("LSR", _executor.LogicalShiftRight, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("JMP", _executor.Jump, _addressingModes.Absolute, 3));
      _instructions.Add(new CpuInstruction("EOR", _executor.ExclusiveOr, _addressingModes.Absolute, 4));
      _instructions.Add(new CpuInstruction("LSR", _executor.LogicalShiftRight, _addressingModes.Absolute, 6));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 6));
      _instructions.Add(new CpuInstruction("BVC", _executor.BranchOnOverflowClear, _addressingModes.Relative, 2));
      _instructions.Add(new CpuInstruction("EOR", _executor.ExclusiveOr, _addressingModes.IndirectY, 5));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 8));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 4));
      _instructions.Add(new CpuInstruction("EOR", _executor.ExclusiveOr, _addressingModes.ZeroPageX, 4));
      _instructions.Add(new CpuInstruction("LSR", _executor.LogicalShiftRight, _addressingModes.ZeroPageX, 6));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 6));
      _instructions.Add(new CpuInstruction("CLI", _executor.ClearInterruptDisable, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("EOR", _executor.ExclusiveOr, _addressingModes.AbsoluteY, 4));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 7));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 4));
      _instructions.Add(new CpuInstruction("EOR", _executor.ExclusiveOr, _addressingModes.AbsoluteX, 4));
      _instructions.Add(new CpuInstruction("LSR", _executor.LogicalShiftRight, _addressingModes.AbsoluteX, 7));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 7));
      _instructions.Add(new CpuInstruction("RTS", _executor.ReturnFromSubroutine, _addressingModes.Implicit, 6));
      _instructions.Add(new CpuInstruction("ADC", _executor.AddWithCarry, _addressingModes.IndirectX, 6));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 8));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 3));
      _instructions.Add(new CpuInstruction("ADC", _executor.AddWithCarry, _addressingModes.ZeroPageZero, 3));
      _instructions.Add(new CpuInstruction("ROR", _executor.RotateRight, _addressingModes.ZeroPageZero, 5));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 5));
      _instructions.Add(new CpuInstruction("PLA", _executor.PullAccumulator, _addressingModes.Implicit, 4));
      _instructions.Add(new CpuInstruction("ADC", _executor.AddWithCarry, _addressingModes.Immediate, 2));
      _instructions.Add(new CpuInstruction("ROR", _executor.RotateRight, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("JMP", _executor.Jump, _addressingModes.Indirect, 5));
      _instructions.Add(new CpuInstruction("ADC", _executor.AddWithCarry, _addressingModes.Absolute, 4));
      _instructions.Add(new CpuInstruction("ROR", _executor.RotateRight, _addressingModes.Absolute, 6));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 6));
      _instructions.Add(new CpuInstruction("BVS", _executor.BranchOnOverflowSet, _addressingModes.Relative, 2));
      _instructions.Add(new CpuInstruction("ADC", _executor.AddWithCarry, _addressingModes.IndirectY, 5));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 8));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 4));
      _instructions.Add(new CpuInstruction("ADC", _executor.AddWithCarry, _addressingModes.ZeroPageX, 4));
      _instructions.Add(new CpuInstruction("ROR", _executor.RotateRight, _addressingModes.ZeroPageX, 6));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 6));
      _instructions.Add(new CpuInstruction("SEI", _executor.SetInterruptDisable, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("ADC", _executor.AddWithCarry, _addressingModes.AbsoluteY, 4));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 7));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 4));
      _instructions.Add(new CpuInstruction("ADC", _executor.AddWithCarry, _addressingModes.AbsoluteX, 4));
      _instructions.Add(new CpuInstruction("ROR", _executor.RotateRight, _addressingModes.AbsoluteX, 7));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 7));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("STA", _executor.StoreAccumulator, _addressingModes.IndirectX, 6));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 6));
      _instructions.Add(new CpuInstruction("STY", _executor.StoreY, _addressingModes.ZeroPageZero, 3));
      _instructions.Add(new CpuInstruction("STA", _executor.StoreAccumulator, _addressingModes.ZeroPageZero, 3));
      _instructions.Add(new CpuInstruction("STX", _executor.StoreX, _addressingModes.ZeroPageZero, 3));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 3));
      _instructions.Add(new CpuInstruction("DEY", _executor.DecrementY, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("TXA", _executor.TransferXToAccumulator, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("STY", _executor.StoreY, _addressingModes.Absolute, 4));
      _instructions.Add(new CpuInstruction("STA", _executor.StoreAccumulator, _addressingModes.Absolute, 4));
      _instructions.Add(new CpuInstruction("STX", _executor.StoreX, _addressingModes.Absolute, 4));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 4));
      _instructions.Add(new CpuInstruction("BCC", _executor.BranchOnCarryClear, _addressingModes.Relative, 2));
      _instructions.Add(new CpuInstruction("STA", _executor.StoreAccumulator, _addressingModes.IndirectY, 6));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 6));
      _instructions.Add(new CpuInstruction("STY", _executor.StoreY, _addressingModes.ZeroPageX, 4));
      _instructions.Add(new CpuInstruction("STA", _executor.StoreAccumulator, _addressingModes.ZeroPageX, 4));
      _instructions.Add(new CpuInstruction("STX", _executor.StoreX, _addressingModes.ZeroPageY, 4));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 4));
      _instructions.Add(new CpuInstruction("TYA", _executor.TransferYToAccumulator, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("STA", _executor.StoreAccumulator, _addressingModes.AbsoluteY, 5));
      _instructions.Add(new CpuInstruction("TXS", _executor.TransferXToStackPointer, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 5));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 5));
      _instructions.Add(new CpuInstruction("STA", _executor.StoreAccumulator, _addressingModes.AbsoluteX, 5));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 5));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 5));
      _instructions.Add(new CpuInstruction("LDY", _executor.LoadY, _addressingModes.Immediate, 2));
      _instructions.Add(new CpuInstruction("LDA", _executor.LoadAccumulator, _addressingModes.IndirectX, 6));
      _instructions.Add(new CpuInstruction("LDX", _executor.LoadX, _addressingModes.Immediate, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 6));
      _instructions.Add(new CpuInstruction("LDY", _executor.LoadY, _addressingModes.ZeroPageZero, 3));
      _instructions.Add(new CpuInstruction("LDA", _executor.LoadAccumulator, _addressingModes.ZeroPageZero, 3));
      _instructions.Add(new CpuInstruction("LDX", _executor.LoadX, _addressingModes.ZeroPageZero, 3));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 3));
      _instructions.Add(new CpuInstruction("TAY", _executor.TransferAccumulatorToY, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("LDA", _executor.LoadAccumulator, _addressingModes.Immediate, 2));
      _instructions.Add(new CpuInstruction("TAX", _executor.TransferAccumulatorToX, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("LDY", _executor.LoadY, _addressingModes.Absolute, 4));
      _instructions.Add(new CpuInstruction("LDA", _executor.LoadAccumulator, _addressingModes.Absolute, 4));
      _instructions.Add(new CpuInstruction("LDX", _executor.LoadX, _addressingModes.Absolute, 4));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 4));
      _instructions.Add(new CpuInstruction("BCS", _executor.BranchOnCarrySet, _addressingModes.Relative, 2));
      _instructions.Add(new CpuInstruction("LDA", _executor.LoadAccumulator, _addressingModes.IndirectY, 5));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 5));
      _instructions.Add(new CpuInstruction("LDY", _executor.LoadY, _addressingModes.ZeroPageX, 4));
      _instructions.Add(new CpuInstruction("LDA", _executor.LoadAccumulator, _addressingModes.ZeroPageX, 4));
      _instructions.Add(new CpuInstruction("LDX", _executor.LoadX, _addressingModes.ZeroPageY, 4));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 4));
      _instructions.Add(new CpuInstruction("CLV", _executor.ClearOverflow, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("LDA", _executor.LoadAccumulator, _addressingModes.AbsoluteY, 4));
      _instructions.Add(new CpuInstruction("TSX", _executor.TransferStackPointerToX, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 4));
      _instructions.Add(new CpuInstruction("LDY", _executor.LoadY, _addressingModes.AbsoluteX, 4));
      _instructions.Add(new CpuInstruction("LDA", _executor.LoadAccumulator, _addressingModes.AbsoluteX, 4));
      _instructions.Add(new CpuInstruction("LDX", _executor.LoadX, _addressingModes.AbsoluteY, 4));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 4));
      _instructions.Add(new CpuInstruction("CPY", _executor.CompareWithY, _addressingModes.Immediate, 2));
      _instructions.Add(new CpuInstruction("CMP", _executor.CompareWithAccumulator, _addressingModes.IndirectX, 6));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 8));
      _instructions.Add(new CpuInstruction("CPY", _executor.CompareWithY, _addressingModes.ZeroPageZero, 3));
      _instructions.Add(new CpuInstruction("CMP", _executor.CompareWithAccumulator, _addressingModes.ZeroPageZero, 3));
      _instructions.Add(new CpuInstruction("DEC", _executor.Decrement, _addressingModes.ZeroPageZero, 5));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 5));
      _instructions.Add(new CpuInstruction("INY", _executor.IncrementY, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("CMP", _executor.CompareWithAccumulator, _addressingModes.Immediate, 2));
      _instructions.Add(new CpuInstruction("DEX", _executor.DecrementX, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("CPY", _executor.CompareWithY, _addressingModes.Absolute, 4));
      _instructions.Add(new CpuInstruction("CMP", _executor.CompareWithAccumulator, _addressingModes.Absolute, 4));
      _instructions.Add(new CpuInstruction("DEC", _executor.Decrement, _addressingModes.Absolute, 6));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 6));
      _instructions.Add(new CpuInstruction("BNE", _executor.BranchOnNotEqual, _addressingModes.Relative, 2));
      _instructions.Add(new CpuInstruction("CMP", _executor.CompareWithAccumulator, _addressingModes.IndirectY, 5));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 8));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 4));
      _instructions.Add(new CpuInstruction("CMP", _executor.CompareWithAccumulator, _addressingModes.ZeroPageX, 4));
      _instructions.Add(new CpuInstruction("DEC", _executor.Decrement, _addressingModes.ZeroPageX, 6));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 6));
      _instructions.Add(new CpuInstruction("CLD", _executor.ClearDecimal, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("CMP", _executor.CompareWithAccumulator, _addressingModes.AbsoluteY, 4));
      _instructions.Add(new CpuInstruction("NOP", _executor.NoOperation, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 7));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 4));
      _instructions.Add(new CpuInstruction("CMP", _executor.CompareWithAccumulator, _addressingModes.AbsoluteX, 4));
      _instructions.Add(new CpuInstruction("DEC", _executor.Decrement, _addressingModes.AbsoluteX, 7));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 7));
      _instructions.Add(new CpuInstruction("CPX", _executor.CompareWithX, _addressingModes.Immediate, 2));
      _instructions.Add(new CpuInstruction("SBC", _executor.SubtractWithCarry, _addressingModes.IndirectX, 6));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 8));
      _instructions.Add(new CpuInstruction("CPX", _executor.CompareWithX, _addressingModes.ZeroPageZero, 3));
      _instructions.Add(new CpuInstruction("SBC", _executor.SubtractWithCarry, _addressingModes.ZeroPageZero, 3));
      _instructions.Add(new CpuInstruction("INC", _executor.Increment, _addressingModes.ZeroPageZero, 5));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 5));
      _instructions.Add(new CpuInstruction("INX", _executor.IncrementX, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("SBC", _executor.SubtractWithCarry, _addressingModes.Immediate, 2));
      _instructions.Add(new CpuInstruction("NOP", _executor.NoOperation, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.SubtractWithCarry, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("CPX", _executor.CompareWithX, _addressingModes.Absolute, 4));
      _instructions.Add(new CpuInstruction("SBC", _executor.SubtractWithCarry, _addressingModes.Absolute, 4));
      _instructions.Add(new CpuInstruction("INC", _executor.Increment, _addressingModes.Absolute, 6));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 6));
      _instructions.Add(new CpuInstruction("BEQ", _executor.BranchOnEqual, _addressingModes.Relative, 2));
      _instructions.Add(new CpuInstruction("SBC", _executor.SubtractWithCarry, _addressingModes.IndirectY, 5));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 8));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 4));
      _instructions.Add(new CpuInstruction("SBC", _executor.SubtractWithCarry, _addressingModes.ZeroPageX, 4));
      _instructions.Add(new CpuInstruction("INC", _executor.Increment, _addressingModes.ZeroPageX, 6));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 6));
      _instructions.Add(new CpuInstruction("SED", _executor.SetDecimal, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("SBC", _executor.SubtractWithCarry, _addressingModes.AbsoluteY, 4));
      _instructions.Add(new CpuInstruction("NOP", _executor.NoOperation, _addressingModes.Implicit, 2));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 7));
      _instructions.Add(new CpuInstruction("???", _executor.NoOperation, _addressingModes.Implicit, 4));
      _instructions.Add(new CpuInstruction("SBC", _executor.SubtractWithCarry, _addressingModes.AbsoluteX, 4));
      _instructions.Add(new CpuInstruction("INC", _executor.Increment, _addressingModes.AbsoluteX, 7));
      _instructions.Add(new CpuInstruction("???", _executor.IllegalOpCode, _addressingModes.Implicit, 7));

      _logger.LogInformation("Built instructions list");
    }
  }
}
